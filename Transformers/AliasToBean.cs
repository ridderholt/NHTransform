using System;
using System.Collections;
using System.Reflection;
using NHibernate;
using NHibernate.Properties;
using NHibernate.Transform;

namespace NHTransform.Transformers
{
    public class CustomAliasToBeanResultTransformer : AliasedTupleSubsetResultTransformer
    {
        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly System.Type resultClass;
        private ISetter[] setters;
        private readonly IPropertyAccessor propertyAccessor;
        private readonly ConstructorInfo constructor;

        public CustomAliasToBeanResultTransformer(System.Type resultClass)
        {
            if (resultClass == null)
            {
                throw new ArgumentNullException("resultClass");
            }
            this.resultClass = resultClass;

            constructor = resultClass.GetConstructor(flags, null, System.Type.EmptyTypes, null);

            // if resultClass is a ValueType (struct), GetConstructor will return null... 
            // in that case, we'll use Activator.CreateInstance instead of the ConstructorInfo to create instances
            if (constructor == null && resultClass.IsClass)
            {
                throw new ArgumentException(
                    "The target class of a AliasToBeanResultTransformer need a parameter-less constructor",
                    "resultClass");
            }

            propertyAccessor =
                new ChainedPropertyAccessor(new[]
                {
                    PropertyAccessorFactory.GetPropertyAccessor(null),
                    PropertyAccessorFactory.GetPropertyAccessor("field")
                });
        }


        public override bool IsTransformedValueATupleElement(String[] aliases, int tupleLength)
        {
            return false;
        }


        public override object TransformTuple(object[] tuple, String[] aliases)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException("aliases");
            }
            object result;

            try
            {
                if (setters == null)
                {
                    setters = new ISetter[aliases.Length];
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        string alias = aliases[i];
                        if (alias != null)
                        {
                            if (IsRelation(alias))
                            {
                                var relationClassInfo = GetPropertyName(alias);
                                setters[i] = propertyAccessor.GetSetter(resultClass, relationClassInfo.ClassType);
                            }
                            else
                            {
                                setters[i] = propertyAccessor.GetSetter(resultClass, alias);
                            }
                        }
                    }
                }

                // if resultClass is not a class but a value type, we need to use Activator.CreateInstance
                result = resultClass.IsClass
                    ? constructor.Invoke(null)
                    : Activator.CreateInstance(resultClass);

                for (int i = 0; i < aliases.Length; i++)
                {
                    if (setters[i] != null && tuple[i] != null)
                    {
                        if (IsRelation(aliases[i]))
                        {
                            object relation = null;
                            var relationClassInfo = GetPropertyName(aliases[i]);
                            var propertyInfo = resultClass.GetProperty(relationClassInfo.ClassType);

                            if (!TryGetProperty(relationClassInfo, result, out relation))
                            {
                                relation = Activator.CreateInstance(propertyInfo.PropertyType);
                            }

                            var relationSetter = propertyAccessor.GetSetter(propertyInfo.PropertyType, relationClassInfo.PropertyName);

                            relationSetter.Set(relation, tuple[i]);
                            
                            setters[i].Set(result, relation);
                        }
                        else
                        {
                            setters[i].Set(result, tuple[i]);
                        }
                    }
                }
            }
            catch (InstantiationException e)
            {
                throw new HibernateException("Could not instantiate result class: " + resultClass.FullName, e);
            }
            catch (MethodAccessException e)
            {
                throw new HibernateException("Could not instantiate result class: " + resultClass.FullName, e);
            }

            return result;
        }

        private bool TryGetProperty(RelationClassInfo relationClassInfo, object result, out object value)
        {
            var someValue = resultClass.GetProperty(relationClassInfo.ClassType).GetValue(result);

            if (someValue != null)
            {
                value = someValue;
                return true;
            }

            value = null;
            return false;
        }

        public override IList TransformList(IList collection)
        {
            return collection;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CustomAliasToBeanResultTransformer);
        }

        public bool Equals(CustomAliasToBeanResultTransformer other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(other.resultClass, resultClass);
        }

        public override int GetHashCode()
        {
            return resultClass.GetHashCode();
        }

        private static bool IsRelation(string propertyName)
        {
            return propertyName.Contains(".");
        }

        private static RelationClassInfo GetPropertyName(string propertyName)
        {
            var values = propertyName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

            return new RelationClassInfo
            {
                PropertyName = values[1],
                ClassType = values[0]
            };

        }


    }

    public abstract class AliasedTupleSubsetResultTransformer : ITupleSubsetResultTransformer
    {
        public abstract bool IsTransformedValueATupleElement(string[] aliases, int tupleLength);

        public bool[] IncludeInTransform(string[] aliases, int tupleLength)
        {
            if (aliases == null)
                throw new ArgumentNullException("aliases");

            if (aliases.Length != tupleLength)
            {
                throw new ArgumentException(
                    "aliases and tupleLength must have the same length; " +
                    "aliases.length=" + aliases.Length + "tupleLength=" + tupleLength
                    );
            }
            bool[] includeInTransform = new bool[tupleLength];
            for (int i = 0; i < aliases.Length; i++)
            {
                if (aliases[i] != null)
                {
                    includeInTransform[i] = true;
                }
            }
            return includeInTransform;
        }

        public abstract object TransformTuple(object[] tuple, string[] aliases);
        public abstract IList TransformList(IList collection);
    }

    public interface ITupleSubsetResultTransformer : IResultTransformer
    {
        /// <summary>
        /// When a tuple is transformed, is the result a single element of the tuple?
        /// </summary>
        /// <param name="aliases">The aliases that correspond to the tuple.</param>
        /// <param name="tupleLength">The number of elements in the tuple.</param>
        /// <returns>True, if the transformed value is a single element of the tuple;
        ///        false, otherwise.</returns>
        bool IsTransformedValueATupleElement(string[] aliases, int tupleLength);


        /// <summary>
        /// Returns an array with the i-th element indicating whether the i-th
        /// element of the tuple is included in the transformed value.
        /// </summary>
        /// <param name="aliases">The aliases that correspond to the tuple.</param>
        /// <param name="tupleLength">The number of elements in the tuple.</param>
        /// <returns>Array with the i-th element indicating whether the i-th
        ///        element of the tuple is included in the transformed value.</returns>
        bool[] IncludeInTransform(string[] aliases, int tupleLength);
    }


    internal class RelationClassInfo
    {
        public string ClassType { get; set; }
        public string PropertyName { get; set; }
    }
}