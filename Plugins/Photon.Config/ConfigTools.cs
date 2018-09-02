using Microsoft.Web.XmlTransform;
using System;

namespace Photon.Config
{
    public static class ConfigTools
    {


        public static void Transform(string configFilename, string transformFilename, string outputFilename)
        {
            using (var configDoc = new XmlTransformableDocument()) {
                configDoc.PreserveWhitespace = true;
                configDoc.Load(configFilename);

                using (var transformDoc = new XmlTransformation(transformFilename)) {
                    if (!transformDoc.Apply(configDoc))
                        throw new ApplicationException("Failed to apply config transformation!");
                }

                configDoc.Save(outputFilename);
            }
        }
    }
}
