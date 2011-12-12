using OpenWrap;
using OpenWrap.Build;
using Tests.version.generation;

namespace Tests.contexts
{
    public class version
    {
        protected string last_build;

        protected SemanticVersion v(string versionIdentifier)
        {
            return SemanticVersion.TryParseExact(versionIdentifier);
        }

        public string ver_build(string builder, int lastBuild = -1)
        {
            return new SemanticVersionGenerator(builder, lastBuild.ToString, build=>last_build=build).Version().ToString();
        }
    }
}