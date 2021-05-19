namespace Database.Services
{
    public interface IDatabaseProvider
    {
        IDemoRepository DemoRepository { get; }
    }

    public class DatabaseProviderService : IDatabaseProvider
    {
        private SqLiteDemoRepository _demoRepository;

        public DatabaseProviderService()
        {
        }

        public IDemoRepository DemoRepository
        {
            get
            {
                if (_demoRepository is null)
                {
                    _demoRepository = new SqLiteDemoRepository();
                    _demoRepository.Init();
                }

                return _demoRepository;
            }
        }
    }

}