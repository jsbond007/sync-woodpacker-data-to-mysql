using ServiceStack.OrmLite;
using System;
using System.Data;

namespace CampaignSync.DataAccess.SyncService.Common
{
    public class DatabaseContext : IDisposable
    {
        private string _connectionString;
        private string _transactionId;
        private bool _matchTransactionId = false;
        private OrmLiteConnectionFactory _dbConnectionFactory;
        public bool TransInitialized { get; private set; }

        public DatabaseContext(OrmLiteConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }

        private IDbConnection _connection;
        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                    this._connection = _dbConnectionFactory.Open();

                if (_connection?.State == ConnectionState.Closed)
                    this._connection = _dbConnectionFactory.Open();

                return _connection;
            }
            set
            {
                _connection = value;
            }
        }
        private async void OpenConnection()
        {
            this.Connection = await this._dbConnectionFactory.OpenAsync();
        }

        public IDbTransaction _currentTransaction;
        public IDbTransaction CurrentTransaction
        {
            get
            {
                return _currentTransaction;
            }
            private set
            {
                this._currentTransaction = value;
            }
        }

        public string BeginTransaction(bool matchTransactionId = false)
        {

            if (this._matchTransactionId != true)
                this._matchTransactionId = matchTransactionId;

            /* if there is no existing transaction going on*/
            if (this.CurrentTransaction == null)
            {
                /* so this transaction is initialized by this function 
                 * commit it after all CUD is done
                 */
                this.CurrentTransaction = this.Connection.BeginTransaction();
                this.TransInitialized = true;
                _transactionId = Guid.NewGuid().ToString();
            }

            return _transactionId;
        }

        public void CommitTransaction(string transactionId = null)
        {
            if (this.TransInitialized && this.CurrentTransaction != null)
            {
                if (!this._matchTransactionId || (this._matchTransactionId && transactionId == this._transactionId))
                {
                    this.CurrentTransaction.Commit();

                    this._matchTransactionId = false;
                    this._transactionId = "";
                    this.CurrentTransaction = null;
                }
            }
        }

        public void RollbackTransaction(string transactionId = null)
        {
            if (this.TransInitialized && this.CurrentTransaction != null)
            {
                if (!this._matchTransactionId || (this._matchTransactionId && transactionId == this._transactionId))
                {
                    this.CurrentTransaction.Rollback();

                    this._matchTransactionId = false;
                    this._transactionId = "";
                    this.CurrentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            if (this._connection != null)
            {
                this._connection.Close();
                this._connection = null;
            }
        }
    }
}