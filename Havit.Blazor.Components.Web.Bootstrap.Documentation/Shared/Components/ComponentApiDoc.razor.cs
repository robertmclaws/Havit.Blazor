﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Xml.Linq;
using System.Xml.XPath;
using LoxSmoke.DocXml;
using System.Text.RegularExpressions;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web;

namespace Havit.Blazor.Components.Web.Bootstrap.Documentation.Shared.Components
{
	public partial class ComponentApiDoc
	{
		private static readonly (string type, string name)[] typeNames =
		{
			new() { type = "Int16",   name = "short"   },
			new() { type = "UInt16",  name = "ushort"  },
			new() { type = "Int32",   name = "int"     },
			new() { type = "UInt32",  name = "uint"    },
			new() { type = "Int64",   name = "long"    },
			new() { type = "UInt64",  name = "ulong"   },
			new() { type = "Boolean", name = "bool"    },
			new() { type = "String",  name = "string"  },
			new() { type = "Char",    name = "char"    },
			new() { type = "Decimal", name = "decimal" },
			new() { type = "Double",  name = "double"  },
			new() { type = "Single",  name = "float"   },
			new() { type = "Byte",    name = "byte"    },
			new() { type = "Sbyte",   name = "sbyte"   },
			new() { type = "Void",    name = "void"    },
			new() { type = "Object",  name = "object"  }
		};

		private static readonly List<string> byDefaultExcludedProperties = new() { "JSRuntime" };
		private static readonly List<string> objectDerivedMethods = new() { "ToString", "GetType", "Equals", "GetHashCode" };
		private static readonly List<string> derivedMethods = new() { "Dispose", "DisposeAsync", "SetParametersAsync", "ChildContent" };

		[Parameter] public RenderFragment ChildContent { get; set; }

		[Parameter] public RenderFragment MainContent { get; set; }

		[Parameter] public RenderFragment CssVariables { get; set; }

		/// <summary>
		/// A type to generate the documentation for
		/// </summary>
		[Parameter] public Type Type { get; set; }

		/// <summary>
		/// Names of members that will be excluded from the displayed documentation
		/// </summary>
		[Parameter] public List<string> ExcludedMembers { get; set; } = new();

		[Parameter] public bool Delegate { get; set; }

		[Inject] private NavigationManager NavigationManager { get; set; }

		private static readonly HttpClient client = new HttpClient();

		private ClassMember classMember;

		private List<Property> properties = new();
		private List<Property> parameters = new();
		private List<Property> staticProperties = new();
		private List<Property> events = new();

		private List<Method> methods = new();
		private List<Method> staticMethods = new();

		private List<EnumMember> enumMembers = new();

		private string delegateSignature;

		private bool isEnum;

		protected override void OnParametersSet()
		{
			DownloadFileAndGetSummaries();
		}

		private void DownloadFileAndGetSummaries()
		{
			TextReader textReader = new StringReader(GetSummaryFileContent());
			XPathDocument xPathDocument = new(textReader);

			DocXmlReader reader = new(xPathDocument);

			classMember = GetClassMember(reader);
			var properties = GetProperties(reader);
			this.properties = properties.properties;
			parameters = properties.parameters;
			staticProperties = properties.staticProperties;
			events = properties.events;

			var methods = GetMethods(reader);
			this.methods = methods.methods;
			staticMethods = methods.staticMethods;

			HandleEnum(reader);
			HandleDelegate();

			StateHasChanged();
		}

		private string GetSummaryFileContent()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = "";

			if (string.IsNullOrEmpty(Type.Namespace))
			{
				resourceName = "Havit.Blazor.Components.Web.Bootstrap.xml";
			}
			else if (Type.Namespace.Contains("Havit.Blazor.GoogleTagManager"))
			{
				resourceName = "Havit.Blazor.GoogleTagManager.xml";
			}
			else if (Type.Namespace.Contains("Bootstrap"))
			{
				resourceName = "Havit.Blazor.Components.Web.Bootstrap.xml";
			}
			else if (Type.Namespace == "Havit.Blazor.Components.Web")
			{
				resourceName = "Havit.Blazor.Components.Web.xml";
			}
			else
			{
				resourceName = "Havit.Blazor.Components.Web.Bootstrap.xml";
			}

			resourceName = assembly.GetManifestResourceNames()
				.Single(str => str.EndsWith(resourceName));

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		private void HandleDelegate()
		{
			if (!Delegate)
			{
				return;
			}

			MethodInfo method = Type.GetMethod("Invoke");
			delegateSignature = $"{FormatType(method.ReturnType.ToString())} {FormatType(Type)} (";
			foreach (ParameterInfo param in method.GetParameters())
			{
				delegateSignature += $"{FormatType(param.ParameterType)} {param.Name}";
			}
			delegateSignature += ")";
			delegateSignature = delegateSignature.Replace("object", "TItem");
		}

		private void HandleEnum(DocXmlReader reader)
		{
			isEnum = Type.IsEnum;
			if (!isEnum)
			{
				return;
			}

			string[] names = Type.GetEnumNames();
			EnumComments enumComments = reader.GetEnumComments(Type);
			for (int i = 0; i < names.Length; i++)
			{
				EnumMember enumMember = new();
				enumMember.Name = names[i];
				try { enumMember.Index = (int)Enum.Parse(Type, enumMember.Name); } catch { }
				try
				{
					var enumValueComment = enumComments.ValueComments.Where(o => o.Name == enumMember.Name).ToList().FirstOrDefault();
					if (enumValueComment is not null)
					{
						enumMember.Summary = enumValueComment.Summary;
					}
				}
				catch { }
				enumMembers.Add(enumMember);
			}
		}

		private ClassMember GetClassMember(DocXmlReader reader)
		{
			return new() { Comments = reader.GetTypeComments(Type) };
		}

		private (List<Property> properties, List<Property> parameters, List<Property> staticProperties, List<Property> events) GetProperties(DocXmlReader reader)
		{
			List<Property> typeProperties = new();
			List<Property> parameters = new();
			List<Property> staticProperties = new();
			List<Property> events = new();

			foreach (var property in Type.GetProperties())
			{
				Property newProperty = new();
				newProperty.PropertyInfo = property;

				if (DetermineWhetherPropertyShouldBeAdded(newProperty) == false)
				{
					continue;
				}

				newProperty.Comments = reader.GetMemberComments(property);

				if (string.IsNullOrEmpty(newProperty.Comments.Summary))
				{
					// newProperty.Comments = FindInheritDoc(newProperty, reader); TO-DO
				}

				if (IsEvent(newProperty))
				{
					events.Add(newProperty);
				}
				else if (HasParameterAttribute(newProperty))
				{
					parameters.Add(newProperty);
				}
				else if (IsPropertyStatic(newProperty))
				{
					staticProperties.Add(newProperty);
				}
				else
				{
					typeProperties.Add(newProperty);
				}
			}

			return (typeProperties, parameters, staticProperties, events);
		}

		private (List<Method> methods, List<Method> staticMethods) GetMethods(DocXmlReader reader)
		{
			List<Method> typeMethods = new();
			List<Method> staticMethods = new();

			foreach (var method in Type.GetMethods())
			{
				Method newMethod = new();
				newMethod.MethodInfo = method;
				newMethod.Comments = reader.GetMethodComments(method);

				if (DetermineWhetherMethodShouldBeAdded(newMethod))
				{
					if (newMethod.MethodInfo.IsStatic)
					{
						staticMethods.Add(newMethod);
					}
					else
					{
						typeMethods.Add(newMethod);
					}
				}
			}

			return (typeMethods, staticMethods);
		}

		private bool DetermineWhetherPropertyShouldBeAdded(Property property)
		{
			string name = property.PropertyInfo.Name;
			if (byDefaultExcludedProperties.Contains(name) || ExcludedMembers.Contains(name))
			{
				return false;
			}

			return true;
		}

		private bool DetermineWhetherMethodShouldBeAdded(Method method)
		{
			// don't add a method if it is JSInvokable
			var customAttributes = method.MethodInfo.CustomAttributes.ToList();
			foreach (var attribute in customAttributes)
			{
				if (attribute.AttributeType == typeof(Microsoft.JSInterop.JSInvokableAttribute))
				{
					return false;
				}
			}

			string name = method.MethodInfo.Name;
			if (name.Contains("set") || name.Contains("get") || objectDerivedMethods.Contains(name) || derivedMethods.Contains(name) || ExcludedMembers.Contains(name))
			{
				return false;
			}

			return true;
		}

		private bool HasParameterAttribute(Property property)
		{
			var customAttributes = property.PropertyInfo.CustomAttributes.ToList();
			foreach (var attribute in customAttributes)
			{
				if (attribute.AttributeType == typeof(ParameterAttribute))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsPropertyStatic(Property property)
		{
			return property.PropertyInfo.GetAccessors(false).Any(o => o.IsStatic);
		}

		private bool IsEvent(Property property)
		{
			return property.PropertyInfo.PropertyType == typeof(EventCallback<>) || property.PropertyInfo.PropertyType == typeof(EventCallback) || property.PropertyInfo.PropertyType.ToString().ToLower().Contains("event");
		}

		private CommonComments FindInheritDoc(Property property, DocXmlReader reader)
		{
			Type[] interfaces = Type.GetInterfaces();

			foreach (var currentInterface in interfaces)
			{
				var matchingMember = currentInterface.GetMembers().Where(o => o.Name == property.PropertyInfo.Name).FirstOrDefault();
				return reader.GetMemberComments(matchingMember);
			}

			return null;
		}

		private static string ReplaceTypeNames(string type)
		{
			foreach (var typeName in typeNames)
			{
				type = type.Replace(typeName.type, typeName.name);
			}

			return type;
		}

		public static string FormatType(Type type)
		{
			string typeName = type.FullName;
			if (string.IsNullOrWhiteSpace(typeName))
			{
				return string.Empty;
			}

			typeName = Regex.Replace(typeName, @"[a-zA-Z]*\.", ""); // Remove namespaces

			var provider = CodeDomProvider.CreateProvider("CSharp");
			var reference = new CodeTypeReference(typeName);

			typeName = ReplaceTypeNames(provider.GetTypeOutput(reference));
			typeName = Regex.Replace(typeName, "Nullable<[a-zA-Z]+>", capture => $"{capture.Value[9..^1]}?");

			string internalTypeName = GenerateLinkForInternalType(typeName);
			if (internalTypeName is not null)
			{
				typeName = internalTypeName;
			}
			else
			{
				typeName = HttpUtility.HtmlEncode(typeName);
			}

			return typeName;
		}

		public static string FormatType(string typeName)
		{
			if (string.IsNullOrWhiteSpace(typeName))
			{
				return string.Empty;
			}

			typeName = Regex.Replace(typeName, @"[a-zA-Z]*\.", ""); // Remove namespaces

			var provider = CodeDomProvider.CreateProvider("CSharp");
			var reference = new CodeTypeReference(typeName);

			typeName = ReplaceTypeNames(provider.GetTypeOutput(reference));
			typeName = Regex.Replace(typeName, "Nullable<[a-zA-Z]+>", capture => $"{capture.Value[9..^1]}?");

			string internalTypeName = GenerateLinkForInternalType(typeName);
			if (internalTypeName is not null)
			{
				typeName = internalTypeName;
			}
			else
			{
				typeName = HttpUtility.HtmlEncode(typeName);
			}

			return typeName;
		}

		public static string GenerateLinkForInternalType(string typeName, bool checkForInternal = true, string linkText = null)
		{
			string typeNameForOwnDocumentation = typeName.Replace("?", "");
			bool generic = false;
			if (typeNameForOwnDocumentation.Contains('<'))
			{
				generic = true;

				typeNameForOwnDocumentation = Regex.Replace(typeNameForOwnDocumentation, "^[^<]+", "");
				typeNameForOwnDocumentation = Regex.Replace(typeNameForOwnDocumentation, "<[a-zA-Z]+>", capture => $"{capture.Value[1..^1]}");
			}

			if (!checkForInternal)
			{
				return GenerateLinkTagForInternalType(typeName, typeNameForOwnDocumentation, linkText, generic);
			}

			if (InternalTypeDoc.DetermineIfTypeIsInternal(typeNameForOwnDocumentation))
			{
				return GenerateLinkTagForInternalType(typeName, typeNameForOwnDocumentation, linkText, generic);
			}

			return null;
		}

		private static string GenerateLinkTagForInternalType(string typeName, string typeNameForOwnDocumentation, string linkText, bool generic)
		{
			if (linkText is null)
			{
				linkText = typeName;

				if (generic)
				{
					linkText = typeNameForOwnDocumentation;
				}
			}

			string[] aroundLinkTexts = typeName.Split(typeNameForOwnDocumentation);
			if (generic && aroundLinkTexts.Length == 2)
			{
				return $"{aroundLinkTexts[0]}<a href=\"/types/{HttpUtility.UrlEncode(typeNameForOwnDocumentation)}\">{HttpUtility.HtmlEncode(linkText)}</a>{aroundLinkTexts[^1]}</code>";
			}

			return $"<a href=\"/types/{HttpUtility.UrlEncode(typeNameForOwnDocumentation)}\">{HttpUtility.HtmlEncode(linkText)}</a></code>";
		}

		public static string RemoveSpecialCharacters(string text)
		{
			Regex regex = new("[^a-zA-Z]");
			return regex.Replace(text, "");
		}
	}
}
