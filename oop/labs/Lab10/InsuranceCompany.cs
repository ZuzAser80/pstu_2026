namespace Lab10
{
    class InsuranceCompany : Organisation
    {
        InsuranceType _insuranceType;
        int _clientCount;
        
        public InsuranceCompany() : base()
        {
            _insuranceType = InsuranceType.NONE;
            _clientCount = 0;
        }

        public InsuranceCompany(string address, string name, int money,
            InsuranceType insuranceType, int clients) : base(address, name, money)
        {
            _insuranceType = insuranceType;
            _clientCount = clients;
        }

        public InsuranceCompany(InsuranceCompany mold) : base(mold)
        {
            _insuranceType = mold._insuranceType;
            _clientCount = mold._clientCount;
        }

        public override void Show()
        {
            System.Console.WriteLine($"INSURANCE COMPANY: {Name} at {Address} earning {AnnualMoneyEarned}. \nInsurance type: {_insuranceType}, used by {_clientCount} people");                        
        }

        public override void Init()
        {            
            base.Init();
            _insuranceType = InsuranceType.NONE;
            _clientCount = 0;
        }
    }

    public enum InsuranceType { NONE, CAR, HOUSE, HEALTH, LIFE };
}