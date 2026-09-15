namespace Lab10
{
    class InsuranceCompany : Organisation
    {
        public InsuranceTypeEnum InsuranceType { get; private set; }
        public int ClientCount { get; private set; }
        public int InsuranceFund { get; private set; }
        
        public InsuranceCompany() : base()
        {
            InsuranceType = InsuranceTypeEnum.NONE;
            ClientCount = 0;
            InsuranceFund = 0;
        }

        public InsuranceCompany(string address, string name, int money,
            InsuranceTypeEnum insuranceType, int clients, int insuranceFund) : base(address, name, money)
        {
            InsuranceType = insuranceType;
            ClientCount = clients;
            InsuranceFund = insuranceFund;
        }

        public InsuranceCompany(InsuranceCompany mold) : base(mold)
        {
            InsuranceType = mold.InsuranceType;
            ClientCount = mold.ClientCount;
            InsuranceFund = mold.InsuranceFund;
        }

        public override void Show()
        {
            System.Console.WriteLine($"INSURANCE COMPANY: {Name} at {Address} earning {AnnualMoneyEarned}. \nInsurance type: {InsuranceType}, used by {ClientCount} people, insurance fund: {InsuranceFund}");                        
        }

        public override void Init()
        {            
            base.Init();
            InsuranceType = InsuranceTypeEnum.NONE;
            ClientCount = 0;
            InsuranceFund = 0;            
        }

        public override void RandomInit()
        {
            base.RandomInit();
            InsuranceType = (InsuranceTypeEnum)random.Next(0, 4);
            ClientCount = random.Next(1, 15);
            InsuranceFund = random.Next(1000, 100000);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return ((InsuranceCompany)obj).ClientCount == ClientCount 
                &&  ((InsuranceCompany)obj).InsuranceType == InsuranceType
                && ((InsuranceCompany)obj).InsuranceFund == InsuranceFund;
            }
            return false;
        }
        
    }

    public enum InsuranceTypeEnum { NONE, CAR, HOUSE, HEALTH, LIFE };
}