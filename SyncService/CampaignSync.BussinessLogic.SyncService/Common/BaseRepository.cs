using CampaignSync.DataAccess.SyncService.Common;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignSync.BussinessLogic.SyncService.Common
{
    public abstract class BaseRepository<T>
      where T : class
    {
        private DatabaseContext _dbContext;
        public DatabaseContext DbContext
        {
            get { return _dbContext; }
            set { _dbContext = value; }
        }

        protected IDbConnection Connection
        {
            get
            {
                return this._dbContext.Connection;
            }
        }


        public BaseRepository(BaseValidationErrorCodes errorCodes, DatabaseContext dbContext
                              )
        {
            this.ErrorCodes = errorCodes;
            this.DbContext = dbContext;
        }


        public virtual BaseValidationErrorCodes ErrorCodes
        {
            get; protected set;
        }

        /// <summary>
        /// Validate Model with required attributes of class
        /// </summary>
        /// <param name="allErrors">ModelState.Values.SelectMany(v => v.Errors)</param>
        /// <param name="isValidModelState">ModelState.IsValid</param>
        /// <param name="validateRequiredFields">if validate Required attribute then true else false</param>
        //public virtual void ValidateModel(IEnumerable<ModelError> allErrors, bool isValidModelState, bool validateRequiredFields = true)
        //{
        //    // if ModelState id not valid then throw an exception 
        //    if (!isValidModelState)
        //    {
        //        List<IValidationResult> errors = new List<IValidationResult>();

        //        foreach (var error in allErrors)
        //        {
        //            string errorMessage = error.ErrorMessage;
        //            // if model is null then find an exception with exact message
        //            if ((string.IsNullOrEmpty(errorMessage) || string.IsNullOrWhiteSpace(errorMessage)) && error.Exception != null)
        //            {
        //                errorMessage = error.Exception.Message;
        //                if (error.Exception.InnerException != null)
        //                {
        //                    if (!string.IsNullOrEmpty(error.Exception.InnerException.Message))
        //                        errorMessage += $" See InnerException = {error.Exception.InnerException.Message}";
        //                }
        //                errors.Add(new ValidationCodeResult(errorMessage, 400));
        //            }
        //            if (validateRequiredFields && !string.IsNullOrEmpty(errorMessage) && !string.IsNullOrWhiteSpace(errorMessage))
        //                errors.Add(new ValidationCodeResult(errorMessage, 400));
        //        }
        //        // If errors found in model then throw an exception
        //        if (errors.Count() > 0)
        //            throw new EntityValidationException("Record rejected due to following errors - ", errors);
        //    }
        //}

        /// <summary>
        /// Get Id(Primary Key) from table by using UId of respective table
        /// </summary>
        /// <typeparam name="T2">interface type of table</typeparam>
        /// <param name="uId">UId (unique id) of table</param>
        /// <param name="isValidate">validate and return exception id UId not exist in database</param>
        /// <returns>Id of table if not found then return null</returns>
        public virtual async Task<long?> GetId<T2>(string uId, bool isValidate = true)
        {
            try
            {
                // Get implemented classes from interface type which is passed as generic type T2
                var tableClasses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                            .Where(x => typeof(T2).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();

                // Get first table type from class
                var tableType = tableClasses[0];

                // Get only alias attributes from class type
                var aliasAttibutes = tableType.CustomAttributes.Where(i => i.AttributeType.Name == "AliasAttribute").ToList();

                // Get alias class name from alias attributes
                var aliasTableName = aliasAttibutes.SelectMany(x => x.ConstructorArguments.Select(c => c.Value)).FirstOrDefault();

                // If alias class name exist then use alias class name else use table type name
                string tableName = aliasTableName != null ? aliasTableName.ToString() : tableType.Name;

                // Create query to get data from database
                string query = $"SELECT Id FROM {tableName} WHERE UId = '{uId}'";
                var id = await this.Connection.QuerySingleOrDefaultAsync<long>(query);

                // If result not found using above query then throw exception as per method parameter
                //if (id == 0 && isValidate)
                //    await this.ThrowEntityException(new ValidationCodeResult($"No Record Found in {tableName} for given UId {uId}", 400));

                if (id != 0)
                    return id;
                return null;
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<T> GetById(long id)
        {
            var t = await this.Connection.SingleByIdAsync<T>(id);
            return t;
        }

        public virtual async Task<T> AddNewAsync(T entity)
        {
            //this.SetUserStamp(entity, false);

            long newId = await this.Connection.InsertAsync<T>(entity, true);
            entity = await this.GetById(newId);
            return entity;
        }


        public virtual async Task<T> AddNewEntity(T entity)
        {
            //this.SetUserStamp(entity, false);
            await this.Connection.InsertAsync<T>(entity, false);
            return entity;
        }


        public virtual async Task<T> Update(T entity)
        {
            //this.SetUserStamp(entity, true);

            long newId = await this.Connection.UpdateAsync<T>(entity);
            entity = await this.GetById(newId);
            return entity;
        }

        public virtual async Task<bool> AddAllAsync(IEnumerable<T> entities)
        {
            //this.SetUserStampList(entities, false);
            await this.Connection.InsertAllAsync<T>(entities);
            return true;
        }

        public virtual async Task<int> UpdateOnlyAsync(T entity, SqlExpression<T> onlyFields)
        {
            //this.SetUserStamp(entity, true);

            return await this.Connection.UpdateOnlyAsync(entity, onlyFields);

        }



        public virtual async Task<int> UpdateAsync(T entity)
        {
            //this.SetUserStamp(entity, true);

            return await this.Connection.UpdateAsync(entity);
        }

        //private void SetUserStampList(IEnumerable<T> entities, bool isUpdate)
        //{
        //    foreach (var entity in entities)
        //    {
        //        SetUserStamp(entity, isUpdate);
        //    }
        //}

        //private void SetUserStamp(T entity, bool isUpdate)
        //{

        //    if (entity is IStamp)
        //    {
        //        IStamp stamp = entity as IStamp;
        //        if (!isUpdate)
        //        {
        //            stamp.CreatedBy = LoggedUser.Id;
        //            stamp.CreatedDateTime = DateTime.UtcNow;
        //        }
        //        stamp.ModifiedBy = LoggedUser.Id;
        //        stamp.ModifiedDateTime = DateTime.UtcNow;
        //    }
        //    if (entity is ICreatedStamp && !isUpdate)
        //    {
        //        ICreatedStamp stamp = entity as ICreatedStamp;
        //        stamp.CreatedBy = LoggedUser.Id;
        //        stamp.CreatedDateTime = DateTime.UtcNow;
        //    }
        //    if (entity is IModifiedStamp)
        //    {
        //        IModifiedStamp stamp = entity as IModifiedStamp;
        //        stamp.ModifiedBy = LoggedUser.Id;
        //        stamp.ModifiedDateTime = DateTime.UtcNow;
        //    }

        //    if (entity is IUniqueIdentifier && !isUpdate)
        //    {
        //        IUniqueIdentifier unique = entity as IUniqueIdentifier;
        //        unique.UId = (Guid.NewGuid()).ToString();
        //    }

        //    if (entity is ICreatedDateTimeStamp && !isUpdate)
        //    {
        //        ICreatedDateTimeStamp stamp = entity as ICreatedDateTimeStamp;
        //        stamp.CreatedDateTime = DateTime.UtcNow;
        //    }
        //    if (entity is IModifiedDateTimeStamp)
        //    {
        //        IModifiedDateTimeStamp stamp = entity as IModifiedDateTimeStamp;
        //        stamp.ModifiedDateTime = DateTime.UtcNow;
        //    }

        //}



        public virtual async Task<int> DeleteByIdAsync<TKey>(object id)
        {
            var result = await this.Connection.DeleteByIdAsync<TKey>(id);
            return result;
        }


        public async Task<IEnumerable<T1>> ExecuteStoredProcedureAsync<T1>(string functionName, params object[] parameters)
        {
            string functionParam = "";
            if (parameters != null && parameters.Count() > 0)
            {
                foreach (var parameter in parameters)
                {
                    string param = CreateFunctionParameter(parameter);
                    functionParam += string.IsNullOrEmpty(param) ? "NULL," : $"{param},";
                }
                string results = functionParam.ToString();
                functionParam = results.Substring(0, results.Length - 1);
            }
            string query = $"call {functionName} {functionParam}";
            var result = await this.Connection.QueryAsync<T1>(query);
            return result;
        }



        private string CreateFunctionParameter(object parameter)
        {
            if (parameter == null)
            {
                return "NULL";
            }
            else if (parameter is string)
            {
                string value = parameter.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    return "NULL";
                }
                else
                {
                    return $"'{value.ToString()}'";
                }
            }
            else if (parameter is DateTime)
            {
                DateTime? value = Convert.ToDateTime(parameter);
                object dvalue = value == DateTime.MinValue ? null : value;
                return dvalue == null ? "NULL" : $"'{dvalue.ToString()}'";
            }
            else if (parameter is bool || parameter is long || parameter is int || parameter is decimal || parameter is double || parameter is float)
            {
                return parameter.ToString();
            }
            else
            {
                return "NULL";
            }
        }

        public string CreateWhereStatement<T2>(SqlExpression<T2> query, string whereExpression)
        {
            for (int i = query.Params.Count() - 1; i >= 0; i--)
            {
                var param = CreateFunctionParameter(query.Params[i].Value);
                whereExpression = whereExpression.Replace($"@{i}", $"{query.Params[i].Value}");
            }
            return whereExpression;
        }

    }
}
