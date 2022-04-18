using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pdsr.HealthChecks.Extensions
{
    public static class HealthChecksExtensions
    {
        public static HealthReportEntry GetHealtReportEntry(this HealthReport report, string entryKey)
        {
            var reportEntry = report.Entries.Single(c => c.Key == entryKey).Value;
            return reportEntry;
        }

        public static T GetHealthReportData<T>(this HealthReportEntry entry, string key)
        {
            var value = entry.Data.Single(c => c.Key == key);
            var tType = typeof(T).GetTypeInfo().BaseType;
            var convertor = TypeDescriptor.GetConverter(typeof(T));
            T converted = (T)convertor.ConvertTo(value.Value, typeof(T));
            return converted;
        }
    }
}
