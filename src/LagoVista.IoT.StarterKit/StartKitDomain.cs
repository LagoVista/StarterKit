// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 86ea1ca563f99effbcd8ed75990783f4a7713e308c6aac062aafcf2afe007544
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models.UIMetaData;
using System;

namespace LagoVista.IoT.StarterKit
{
    [DomainDescriptor]
    public class StarterKitDomain
    {
        public const string StarterKit = "StarterKit";


        [DomainDescription(StarterKit)]
        public static DomainDescription StarterKitDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Utilities and methods to make creating IoT applications a little easier.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Starter Kit",
                    CurrentVersion = new LagoVista.Core.Models.VersionInfo()
                    {
                        Major = 0,
                        Minor = 8,
                        Build = 001,
                        DateStamp = new DateTime(2023, 10, 23),
                        Revision = 1,
                        ReleaseNotes = "Initial unstable preview"
                    }
                };
            }
        }
    }
}
