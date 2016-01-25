namespace Brennis.DataMining.Assignments.DataSmartCh6.Model
{
    /// <summary>
    /// Model van klanten en model coefficienten
    /// </summary>
    public class Customer
    {
        public double Male { get; set; }
        public double Female { get; set; }
        public double Home { get; set; }
        public double Apt { get; set; }
        public double PregnancyTest { get; set; }
        public double BirthControl { get; set; }
        public double FeminineHygiene { get; set; }
        public double FolicAcid { get; set; }
        public double PrenatalVitamins { get; set; }
        public double PrenatalYoga { get; set; }
        public double BodyPillow { get; set; }
        public double GingerAle { get; set; }
        public double SeaBands { get; set; }
        public double StoppedBuyingCiggies { get; set; }
        public double Cigarettes { get; set; }
        public double SmokingCessation { get; set; }
        public double StoppedBuyingWhine { get; set; }
        public double Wine { get; set; }
        public double MaternityClothes { get; set; }
        public double Pregnant { get; set; }

        public double Intercept { get; set; }
        public double SumProduct { get; set; }
        public double SSE { get; set; }

        public static Customer Copy(Customer customer)
        {
            return new Customer
            {
                Apt = customer.Apt,
                BirthControl = customer.BirthControl,
                BodyPillow = customer.BodyPillow,
                Cigarettes = customer.Cigarettes,
                Female = customer.Female,
                FeminineHygiene = customer.FeminineHygiene,
                FolicAcid = customer.FolicAcid,
                GingerAle = customer.GingerAle,
                Home = customer.Home,
                Intercept = customer.Intercept,
                SumProduct = customer.SumProduct,
                SSE = customer.SSE,
                Male = customer.Male,
                StoppedBuyingCiggies = customer.StoppedBuyingCiggies,
                Pregnant = customer.Pregnant,
                MaternityClothes = customer.MaternityClothes,
                PregnancyTest = customer.PregnancyTest,
                PrenatalVitamins = customer.PrenatalVitamins,
                PrenatalYoga = customer.PrenatalYoga,
                SeaBands = customer.SeaBands,
                SmokingCessation = customer.SmokingCessation,
                StoppedBuyingWhine = customer.StoppedBuyingWhine,
                Wine = customer.Wine
            };
        }
    }

    
}
