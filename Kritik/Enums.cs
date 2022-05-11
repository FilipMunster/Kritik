using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Kritik
{
    // Values in Description are used in GUI
    public enum BoundaryCondition
    {
        [Description("volný")]
        free,
        [Description("kloub")]
        joint,
        [Description("vetknutí")]
        fix
    }
    public enum ElementType
    {
        [Description("Hřídel")]
        beam,
        [Description("Hřídel+")]
        beamPlus,
        [Description("Tuhý")]
        rigid,
        [Description("Disk")]
        disc,
        [Description("Podpora")]
        support,
        [Description("Magnet. tah")]
        magnet
    }
    public enum GyroscopicEffect
    {
        [Description("zanedbání")]
        none,
        [Description("souběžná precese")]
        forward,
        [Description("protiběžná precese")]
        backward
    }
    public enum ShaftRotationInfluenceOption
    {
        operatingSpeed,
        runawaySpeed,
        custom
    }

    public static class Enums
    {
        /// <summary>
        /// Returns array of names of enum values
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns></returns>
        public static string[] GetNames<T>() where T : Enum
        {
            List<string> names = new List<string>();
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                names.Add(item.GetDescription());
            }
            return names.ToArray();
        }

        public static string[] GetNamesUsingResourceDictionary<T>(ResourceDictionary resourceDictionary) where T : Enum
        {
            List<string> names = new List<string>();
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                names.Add(item.GetNameUsingResourceDictionary(resourceDictionary));
            }
            return names.ToArray();
        }
    }

}
