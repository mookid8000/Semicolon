using System;
using FastMember;

// ReSharper disable ArgumentsStyleLiteral

namespace Semicolon.Extensions
{
    static class MemberExtensions
    {
        public static bool HasAttribute<TAttribute>(this Member member, Func<TAttribute, bool> criteria = null) where TAttribute : Attribute
        {
            var attribute = member.GetAttribute<TAttribute>();
            if (attribute == null) return false;

            return criteria == null || criteria(attribute);
        }

        public static TAttribute GetAttribute<TAttribute>(this Member member) where TAttribute : Attribute
        {
            return member.GetAttribute(typeof(TAttribute), inherit: true) is TAttribute attribute
                ? attribute
                : null;
        }
    }
}