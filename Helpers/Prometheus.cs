namespace dell_switch_exporter
{
    public static class Prometheus
    {
        public static string CreateMetric(string name, string value, string parameters)
        {
            string metric = string.Empty;

            name = string.Format("dell_switch_{0}", name).ToLower();

            metric += string.Format("{0}{2} {1}\n", name, value, parameters);

            return metric;
        }
        public static string CreateMetricDescription(string name, string type, string description)
        {
            string metric = string.Empty;

            name = string.Format("dell_switch_{0}", name).ToLower();

            metric += string.Format("# HELP {0} {1}\n", name, description);
            metric += string.Format("# TYPE {0} {1}\n", name, type);

            return metric;
        }
    }
}