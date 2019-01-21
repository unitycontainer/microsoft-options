using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unity;

namespace Tests.OptionsValidationTest
{
    public partial class OptionsValidationTest
    {
        [TestMethod]
        public void ValidationResultSuccessIfNameMatched()
        {
            // Setup
            Container.Validate<ComplexOptions>(o => o.Boolean)
                     .Validate<ComplexOptions>(o => o.Integer > 12);

            // Act

            var validations = Container.Resolve<IEnumerable<IValidateOptions<ComplexOptions>>>();
            var options = new ComplexOptions
            {
                Boolean = true,
                Integer = 13
            };
            foreach (var v in validations)
            {
                Assert.IsTrue(v.Validate(Options.DefaultName, options).Succeeded);
                Assert.IsTrue(v.Validate("Something", options).Skipped);
            }
        }

        [TestMethod]
        public void ValidationResultSkippedIfNameNotMatched()
        {
            // Setup
            Container.Validate<ComplexOptions>("Name", o => o.Boolean);

            // Act

            var validations = Container.Resolve<IEnumerable<IValidateOptions<ComplexOptions>>>();
            var options = new ComplexOptions
            {
                Boolean = true,
            };
            foreach (var v in validations)
            {
                Assert.IsTrue(v.Validate(Options.DefaultName, options).Skipped);
                Assert.IsTrue(v.Validate("Name", options).Succeeded);
            }
        }

        [TestMethod]
        public void ValidationResultFailedOrSkipped()
        {
            // Setup
            Container.Validate<ComplexOptions>("Name", o => o.Boolean);

            // Act
            var validations = Container.Resolve<IEnumerable<IValidateOptions<ComplexOptions>>>();


            var options = new ComplexOptions
            {
                Boolean = false,
            };
            foreach (var v in validations)
            {
                Assert.IsTrue(v.Validate(Options.DefaultName, options).Skipped);
                Assert.IsTrue(v.Validate("Name", options).Failed);
            }
        }
    }
}
