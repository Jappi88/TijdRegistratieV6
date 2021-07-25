﻿using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

namespace ProductieManager.Misc
{
    public class AssemblyInfo
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Company { get; private set; }
        public string Product { get; private set; }
        public string Copyright { get; private set; }
        public string Trademark { get; private set; }
        public string AssemblyVersion { get; private set; }
        public string FileVersion { get; private set; }
        public string Guid { get; private set; }
        public string NeutralLanguage { get; private set; }
        public bool IsComVisible { get; private set; }

        // Constructors.
        public AssemblyInfo()
            : this(Assembly.GetExecutingAssembly())
        {
        }

        public AssemblyInfo(Assembly assembly)
        {
            // Get values from the assembly.
            AssemblyTitleAttribute titleAttr =
                GetAssemblyAttribute<AssemblyTitleAttribute>(assembly);
            if (titleAttr != null) Title = titleAttr.Title;

            AssemblyDescriptionAttribute assemblyAttr =
                GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly);
            if (assemblyAttr != null) Description =
                assemblyAttr.Description;

            AssemblyCompanyAttribute companyAttr =
                GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly);
            if (companyAttr != null) Company = companyAttr.Company;

            AssemblyProductAttribute productAttr =
                GetAssemblyAttribute<AssemblyProductAttribute>(assembly);
            if (productAttr != null) Product = productAttr.Product;

            AssemblyCopyrightAttribute copyrightAttr =
                GetAssemblyAttribute<AssemblyCopyrightAttribute>(assembly);
            if (copyrightAttr != null) Copyright = copyrightAttr.Copyright;

            AssemblyTrademarkAttribute trademarkAttr =
                GetAssemblyAttribute<AssemblyTrademarkAttribute>(assembly);
            if (trademarkAttr != null) Trademark = trademarkAttr.Trademark;

            AssemblyVersion = assembly.GetName().Version.ToString();

            AssemblyFileVersionAttribute fileVersionAttr =
                GetAssemblyAttribute<AssemblyFileVersionAttribute>(assembly);
            if (fileVersionAttr != null) FileVersion =
                fileVersionAttr.Version;

            GuidAttribute guidAttr = GetAssemblyAttribute<GuidAttribute>(assembly);
            if (guidAttr != null) Guid = guidAttr.Value;

            NeutralResourcesLanguageAttribute languageAttr =
                GetAssemblyAttribute<NeutralResourcesLanguageAttribute>(assembly);
            if (languageAttr != null) NeutralLanguage =
                languageAttr.CultureName;

            ComVisibleAttribute comAttr =
                GetAssemblyAttribute<ComVisibleAttribute>(assembly);
            if (comAttr != null) IsComVisible = comAttr.Value;
        }

        // Return a particular assembly attribute value.
        public static T GetAssemblyAttribute<T>(Assembly assembly)
            where T : Attribute
        {
            // Get attributes of this type.
            object[] attributes =
                assembly.GetCustomAttributes(typeof(T), true);

            // If we didn't get anything, return null.
            if ((attributes == null) || (attributes.Length == 0))
                return null;

            // Convert the first attribute value into
            // the desired type and return it.
            return (T)attributes[0];
        }
    }
}