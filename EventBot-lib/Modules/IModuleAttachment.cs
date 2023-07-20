namespace EventBot.lib.Modules {
    public interface IModuleAttachment<Module> {
        public Module OwningModule { get; set; }
    }
}