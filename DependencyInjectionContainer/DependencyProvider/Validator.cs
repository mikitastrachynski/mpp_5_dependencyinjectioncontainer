﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DependencyInjectionContainer.Configuration;

namespace DependencyInjectionContainer.DependencyProvider
{
    
    public class Validator
    {
        public DependenciesConfiguration DependenciesConfiguration { get; set; }


        public bool Validate()
        {
            bool isValid = true;
            foreach (Type dependency in DependenciesConfiguration.Dependencies.Keys.ToList())
            {
                isValid = isValid && IsValid(dependency);
            }

            return isValid;
        }

        public bool IsValid(Type dependency)
        {
            bool isValid = true;
            if (dependency.GetGenericArguments().ToList().Count == 0)
            {
                isValid = IsNonGenericValid(dependency);
            }
            else
            {
                isValid = IsGenericValid(dependency);
            }

            return isValid;
        }

        private bool IsConstructorValid(Implementation implementation)
        {
            Console.WriteLine(implementation.ImplementationType.FullName);
            List<ConstructorInfo> constructors = implementation.ImplementationType.GetConstructors().ToList();
            bool isValid = false;
            foreach (ConstructorInfo constructor in constructors)
            {
                List<ParameterInfo> parameters = constructor.GetParameters().ToList();
                bool isParametersValid = true;
                foreach (ParameterInfo parameter in parameters)
                {
                    int index = DependenciesConfiguration.Dependencies.Keys.ToList()
                        .FindIndex(x => x.GetType() == parameter.GetType());
                    if (index == -1)
                        isParametersValid = false;
                    else
                        isParametersValid = IsValid(DependenciesConfiguration.Dependencies.Keys.ToList()[index]);
                }
                isValid = isValid || isParametersValid;
            }
            return isValid;
        }

        public bool IsNonGenericValid(Type dependency)
        {
            bool isValid = true;
            if (DependenciesConfiguration.Dependencies.Keys.ToList().Exists(x => x.FullName == dependency.FullName))
                foreach (Implementation implementation in DependenciesConfiguration.Dependencies[dependency])
                {
                    isValid = isValid && implementation.ImplementationType.IsAssignableFrom(dependency) ||
                              implementation
                                  .ImplementationType.GetInterfaces()
                                  .Any(x => x.ToString() == dependency.ToString());
                    isValid = isValid && IsConstructorValid(implementation);
                }
            else
            {
                isValid = false;
            }

            return isValid;
        }

        public bool IsGenericValid(Type dependency)
        {
            bool isValid = true;
            string dependencyName = dependency.FullName.Split('[')[0];
            if (DependenciesConfiguration.Dependencies.Keys.ToList().Exists(x => x.FullName == dependencyName))
            {
                Type depend = DependenciesConfiguration.Dependencies.Keys.ToList().Find(x => x.FullName == dependencyName);
                foreach (Implementation implementation in DependenciesConfiguration.Dependencies[depend])
                {
                    isValid = isValid && IsValid(dependency.GetGenericArguments().ToList()[0]);
                }
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }
    }
}