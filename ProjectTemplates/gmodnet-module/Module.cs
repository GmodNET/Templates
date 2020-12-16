using GmodNET.API;

namespace TemplateModule
{
    public class Module : IModule
    {
        public string ModuleName => "TemplateModule";

        public string ModuleVersion => "1.0.0";

        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.GetField(-1, "print");
            lua.PushString("Hello World!");
            lua.MCall(1, 0);
            lua.Pop();
        }

        public void Unload(ILua lua)
        {
            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.GetField(-1, "print");
            lua.PushString("Goodbye World!");
            lua.MCall(1, 0);
            lua.Pop();
        }
    }
}
