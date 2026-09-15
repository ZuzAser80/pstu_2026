namespace Lab10
{
    class InsuranceCompany : Organisation
    {
        public InsuranceTypeEnum InsuranceType { get; private set; }
        public int ClientCount { get; private set; }
        
        public InsuranceCompany() : base()
        {
            InsuranceType = InsuranceTypeEnum.NONE;
            ClientCount = 0;
        }

        public InsuranceCompany(string address, string name, int money,
            InsuranceTypeEnum insuranceType, int clients) : base(address, name, money)
        {
            InsuranceType = insuranceType;
            ClientCount = clients;
        }

        public InsuranceCompany(InsuranceCompany mold) : base(mold)
        {
            InsuranceType = mold.InsuranceType;
            ClientCount = mold.ClientCount;
        }

        public override void Show()
        {
            System.Console.WriteLine($"INSURANCE COMPANY: {Name} at {Address} earning {AnnualMoneyEarned}. \nInsurance type: {InsuranceType}, used by {ClientCount} people");                        
        }

        public override void Init()
        {            
            base.Init();
            InsuranceType = InsuranceTypeEnum.NONE;
            ClientCount = 0;            
        }

        public override void RandomInit()
        {
            base.RandomInit();
            InsuranceType = (InsuranceTypeEnum)random.Next(0, 4);
            ClientCount = random.Next(1, 15);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return ((InsuranceCompany)obj).ClientCount == ClientCount 
                &&  ((InsuranceCompany)obj).InsuranceType == InsuranceType;
            }
            return false;
        }
        
    }

    public enum InsuranceTypeEnum { NONE, CAR, HOUSE, HEALTH, LIFE };
}