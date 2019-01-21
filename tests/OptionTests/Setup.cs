﻿using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using Unity;
using Unity.Microsoft.Options;

namespace Tests.OptionTests
{
    [TestClass]
    public partial class OptionTests
    {
        protected IUnityContainer Container;


        [TestInitialize]
        public void Setup()
        {
            Container = new UnityContainer()
                .AddExtension(new OptionsExtension())
                .AddExtension(new Diagnostic());
        }

        public static IEnumerable<object[]> Configure_GetsNullableOptionsFromConfiguration_Data
        {
            get
            {
                yield return new object[]
                {
                    new Dictionary<string, string>
                    {
                        { nameof(NullableOptions.MyNullableBool), "true" },
                        { nameof(NullableOptions.MyNullableInt), "1" },
                        { nameof(NullableOptions.MyNullableDateTime),
                            new DateTime(2015, 1, 1)
                                .ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern) }
                    },
                    new Dictionary<string, object>
                    {
                        { nameof(NullableOptions.MyNullableBool), true },
                        { nameof(NullableOptions.MyNullableInt), 1 },
                        { nameof(NullableOptions.MyNullableDateTime),
                            new DateTime(2015, 1, 1) }
                    }
                };
                yield return new object[]
                {
                    new Dictionary<string, string>
                    {
                        { nameof(NullableOptions.MyNullableBool), "false" },
                        { nameof(NullableOptions.MyNullableInt), "-1" },
                        { nameof(NullableOptions.MyNullableDateTime),
                            new DateTime(1995, 12, 31)
                                .ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern) }
                    },
                    new Dictionary<string, object>
                    {
                        { nameof(NullableOptions.MyNullableBool), false },
                        { nameof(NullableOptions.MyNullableInt), -1 },
                        { nameof(NullableOptions.MyNullableDateTime),
                            new DateTime(1995, 12, 31) }
                    }
                };
                yield return new object[]
                {
                    new Dictionary<string, string>
                    {
                        { nameof(NullableOptions.MyNullableBool), null },
                        { nameof(NullableOptions.MyNullableInt), null },
                        { nameof(NullableOptions.MyNullableDateTime), null }
                    },
                    new Dictionary<string, object>
                    {
                        { nameof(NullableOptions.MyNullableBool), null },
                        { nameof(NullableOptions.MyNullableInt), null },
                        { nameof(NullableOptions.MyNullableDateTime), null }
                    }
                };
            }
        }

        public static IEnumerable<object[]> Configure_GetsEnumOptionsFromConfiguration_Data
        {
            get
            {
                yield return new object[]
                {
                    new Dictionary<string, string>
                    {
                        { nameof(EnumOptions.UriKind), (UriKind.Absolute).ToString() },
                    },
                    new Dictionary<string, object>
                    {
                        { nameof(EnumOptions.UriKind), UriKind.Absolute },
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, string>
                    {
                        { nameof(EnumOptions.UriKind), ((int)UriKind.Absolute).ToString() },
                    },
                    new Dictionary<string, object>
                    {
                        { nameof(EnumOptions.UriKind), UriKind.Absolute },
                    }
                };

                yield return new object[]
                {
                    new Dictionary<string, string>
                    {
                        { nameof(EnumOptions.UriKind), null },
                    },
                    new Dictionary<string, object>
                    {
                        { nameof(EnumOptions.UriKind), UriKind.RelativeOrAbsolute },  //default enum, since not overridden by configuration
                    }
                };
            }
        }

    }

    #region Test Data

    public class FakeOptionsFactory : IOptionsFactory<FakeOptions>
    {
        public static FakeOptions Options = new FakeOptions();

        public FakeOptions Create(string name) => Options;
    }

    public class FakeOptions
    {
        public FakeOptions()
        {
            Message = "";
        }

        public string Message { get; set; }
    }

    public class FakeOptions2 : FakeOptions { }

    public class ComplexOptions
    {
        public ComplexOptions()
        {
            Nested = new NestedOptions();
            Virtual = "complex";
        }
        public NestedOptions Nested { get; set; }
        public int Integer { get; set; }
        public bool Boolean { get; set; }
        public virtual string Virtual { get; set; }

        public string PrivateSetter { get; private set; }
        public string ProtectedSetter { get; protected set; }
        public string InternalSetter { get; internal set; }
        public static string StaticProperty { get; set; }

        public string ReadOnly
        {
            get { return null; }
        }
    }

    public class NestedOptions
    {
        public int Integer { get; set; }
    }

    public class DerivedOptions : ComplexOptions
    {
        public override string Virtual
        {
            get
            {
                return base.Virtual;
            }
            set
            {
                base.Virtual = "Derived:" + value;
            }
        }
    }

    public class NullableOptions
    {
        public bool? MyNullableBool { get; set; }
        public int? MyNullableInt { get; set; }
        public DateTime? MyNullableDateTime { get; set; }
    }

    public class EnumOptions
    {
        public UriKind UriKind { get; set; }
    }

    #endregion
}
