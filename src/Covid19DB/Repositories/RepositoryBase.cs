using Covid19DB.Db;

namespace Covid19DB.Repositories
{
    public class RepositoryBase<T>
    {
        #region Properties
        protected Covid19DbContext Covid19DbContext { get; }
        #endregion

        #region Constructor
        public RepositoryBase(Covid19DbContext covid19DbContext)
        {
            Covid19DbContext = covid19DbContext;
        }
        #endregion

        public void Update(T item)
        {
            _ = Covid19DbContext.Update(item);
        }
    }
}
