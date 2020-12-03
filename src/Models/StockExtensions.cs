using System;

namespace app.Models
{
    public static class StockExtensions
    {
        public static void Merge(this Stock instanceA, Stock instanceB)
        {
            if (instanceA != null && instanceB != null)
            {
                if(instanceB.Code != null)
                {
                    instanceA.Code = instanceB.Code;
                }

                if(instanceB.Time != null)
                {
                    instanceA.Time = instanceB.Time;
                }

                if(instanceB.Open != null)
                {
                    instanceA.Open = instanceB.Open;
                }

                if(instanceB.High != null)
                {
                    instanceA.High = instanceB.High;
                }

                if(instanceB.Low != null)
                {
                    instanceA.Low = instanceB.Low;
                }

                if(instanceB.Close != null)
                {
                    instanceA.Close = instanceB.Close;
                }
            }
        }
    }
}