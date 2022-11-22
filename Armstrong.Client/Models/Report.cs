using Armstrong.Client.Data;
using Armstrong.Client.Repository;
using Armstrong.Client.Utilits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Armstrong.Client.Models
{
    enum DeviceType : int { EquivalenDoseRate = 1, GasVolumetricActivity, AerosolVolumetricActivity, Impulses };
    enum Category : int { SpecialControl = 1, Blowout, Aerosol, Impulses };
    enum BlowoutCategory : int { I = 1, II, III };

    public class Report
    {
        public Channel? Channel { get; set; }
        public List<History>? ReportHistories { get; set; }

        public DateTime StartReportDate { get; set; }
        public DateTime EndReportDate { get; set; } = DateTime.UtcNow;

        public DateTime FactStartReportDate { get; set; }
        public DateTime FactEndReportDate { get; set; }

        public double AverageSystemValue { get; set; } = 0;
        public double AverageNotSystemValue { get; set; } = 0;
        public double BlowoutSystemValue { get; set; } = 0;
        public double BlowoutNotSystemValue { get; set; } = 0;

        public int ReportCategory { get; set; }
        public int BlowoutReportCategory { get; set; } = 0;

        public Report(int id, DateTime startDateTime)
        {
            var repos = new ChannelRepository(new DataContext());
            double convertCoefficient = 37000000000;

            Channel = repos.GetChannel(id);
            StartReportDate = startDateTime;

            if (Channel is null)
            {
                return;
            }

            ReportHistories = GetHistories();

            if (ReportHistories.Any())
            {
                if (Channel.DeviceType is (int)DeviceType.GasVolumetricActivity)
                {
                    ReportCategory = (int)Category.Blowout;

                    SetBlowoutReportCategory();

                    BlowoutSystemValue = GetBlowoutSystemValue();
                    BlowoutNotSystemValue = BlowoutSystemValue / convertCoefficient;

                    AverageSystemValue = GetAverageSystemValue();
                    AverageNotSystemValue = UnitConverter.Convert(type: (int)DeviceType.GasVolumetricActivity,
                                                                          value: AverageSystemValue);
                }
                else
                {
                    AverageSystemValue = GetAverageSystemValue();

                    switch (Channel.DeviceType)
                    {
                        case (int)DeviceType.EquivalenDoseRate:
                            ReportCategory = (int)Category.SpecialControl;
                            AverageNotSystemValue = UnitConverter.Convert(type: (int)DeviceType.EquivalenDoseRate,
                                                                          value: AverageSystemValue);
                            break;
                        case (int)DeviceType.AerosolVolumetricActivity:
                            ReportCategory = (int)Category.Aerosol;
                            AverageNotSystemValue = UnitConverter.Convert(type: (int)DeviceType.AerosolVolumetricActivity,
                                                                          value: AverageSystemValue);
                            break;
                        default:
                            AverageNotSystemValue = AverageSystemValue;
                            break;
                    }
                }
            }
        }

        private void SetBlowoutReportCategory()
        {
            int V1id = 275, V2id = 276, V3id = 277, V4id = 278, V4_id = 279, V5id = 280, V6id = 281, V7id = 282;

            int[] categoryI = new int[] { V1id, V7id };
            int[] categoryII = new int[] { V2id, V3id, V4_id, V5id };
            int[] categoryIII = new int[] { V6id };

            if (categoryI.Contains(Channel.Id))
            {
                BlowoutReportCategory = (int)BlowoutCategory.I;
            }
            else if (categoryII.Contains(Channel.Id))
            {
                BlowoutReportCategory = (int)BlowoutCategory.II;
            }
            else if (categoryIII.Contains(Channel.Id))
            {
                BlowoutReportCategory = (int)BlowoutCategory.III;
            }
        }

        private List<History> GetHistories()
        {
            var histories = new List<History>();

            using (var context = new DataContext())
            {
                histories = context.Histories.AsNoTracking()
                                             .Where(x => x.Id == Channel.Id)
                                             .Where(d => d.EventDate > StartReportDate && d.EventDate < EndReportDate)
                                             .OrderBy(x => x.EventDate)
                                             .ToList();
            }

            if (histories.Any())
            {
                FactStartReportDate = histories.Select(x => x.EventDate).Min();
                FactEndReportDate = histories.Select(x => x.EventDate).Max();
            }

            return histories;
        }

        private double GetBlowoutSystemValue()
        {
            double blowout, summ = 0;

            for (int i = 0; i < ReportHistories.Count - 1; i++)
            {
                DateTime first = ReportHistories[i].EventDate;
                DateTime last = ReportHistories[i + 1].EventDate;

                double valueA = ReportHistories[i].SystemEventValue;
                double valueB = ReportHistories[i + 1].SystemEventValue;
                double avgAB = (valueA + valueB) / 2;

                double substract = last.Subtract(first).TotalSeconds;

                summ += avgAB * substract;
            }

            var consumption = Channel.ChannelConsumption / 3600;
            blowout = summ * consumption;

            return blowout;
        }

        private double GetAverageSystemValue() =>
            ReportHistories.Any() ? ReportHistories.Average(avg => avg.SystemEventValue) : 0;
    }
}
