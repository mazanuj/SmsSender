namespace SmsSender.XmlHelpers.Request
{
    public abstract class RequestParamsBase
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int LifeTime { get; set; }
        public int Rate { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }

        protected RequestParamsBase()
        {
            this.StartTime = this.EndTime = "AUTO";
        }
    }
}
