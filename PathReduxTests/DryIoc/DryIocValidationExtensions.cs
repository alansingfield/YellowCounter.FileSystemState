using DryIoc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PathReduxTests.DryIoc
{
    public static class DryIocValidationExtensions
    {
        public static void ValidateOrThrow(this IContainer container)
        {
            var validateResult = String.Join("\r\n", container.Validate(x => !x.ServiceType.IsOpenGeneric())
                .Select(x => new { x.Key, x.Value })
                .Distinct()
                .Select(x => $"{x.Key} {x.Value}"));

            if(!String.IsNullOrEmpty(validateResult))
                throw new Exception(validateResult);
        }
    }
}
