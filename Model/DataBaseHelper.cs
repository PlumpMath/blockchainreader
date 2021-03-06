using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using MySql.Data.MySqlClient;

namespace blockchain_parser.Model
{
    public abstract class DataBaseHelper
    {
        public DataBaseHelper() {}
		
		protected T Read<T, M>(Func<FynContext, DbSet<T>> query,
			Expression<Func<T, M>> sort_desceding) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				T item = query(db).OrderByDescending(sort_desceding).FirstOrDefault();
				return item;
			});
        }

        protected T Read<T>(Func<FynContext, DbSet<T>> query,
			Expression<Func<T, bool>> condition) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				T item = query(db).Where(condition).FirstOrDefault();
				return item;
			});
        }

        protected List<T> Reads<T>(Func<FynContext, DbSet<T>> query,
			Expression<Func<T, bool>> condition) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				List<T> item = query(db).Where(condition).ToList();
				return item;
			});
        }

		protected List<T> IncludeReads<T, M>(Func<FynContext, DbSet<T>> query,
			Expression<Func<T, M>> include, Expression<Func<T, bool>> condition) 
            where T : class where M : class
		{
			return ExecuteDbTransaction(db =>
			{
				List<T> item = query(db).Include(include).Where(condition).ToList();
				return item;
			});
        }
        protected List<T> NestedIncludeReads<T, M, R>(Func<FynContext, DbSet<T>> query,
			Expression<Func<T, IEnumerable<M>>> include, Expression<Func<M, R>> nested_include, Expression<Func<T, bool>> condition) 
            where T : class where M : class where R : class
		{
			return ExecuteDbTransaction(db =>
			{
				List<T> item = query(db).Include(include).ThenInclude(nested_include).Where(condition).ToList();
				return item;
			});
        }

        protected bool? Update<T>(Func<FynContext, DbSet<T>> query,
			Expression<Func<T, bool>> condition, Action<T> update) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				T item = query(db).Where(condition).FirstOrDefault();
				update(item);
				db.SaveChanges();
				return true;
			});
        }

        protected bool? Create<T>(Func<FynContext, DbSet<T>> query, T item) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				query(db).Add(item);
				db.SaveChanges();
				return true;
			});
		}

		protected bool? Create<T>(Func<FynContext, DbSet<T>> query, IEnumerable<T> item) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				query(db).AddRange(item);
				db.SaveChanges();
				return true;
			});
		}

		protected bool? Delete<T>(Func<FynContext, DbSet<T>> query, T item) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				query(db).Remove(item);
				db.SaveChanges();
				return true;
			});
		}

		protected bool? Delete<T>(Func<FynContext, DbSet<T>> query, IEnumerable<T> item) where T : class
		{
			return ExecuteDbTransaction(db =>
			{
				query(db).RemoveRange(item);
				db.SaveChanges();
				return true;
			});
        }

        protected dynamic ExecuteDbTransaction(Func<FynContext, dynamic> execution)
		{
			using (var db = new FynContext())
			{
				bool save_failed;
				do
				{
					save_failed = false;
					try
					{
						return execution(db);
					}
					catch (DbUpdateConcurrencyException ex)
					{
						save_failed = true;
						ex.Entries.Single().Reload();
					}
					catch (DbUpdateException ex)
					{
						MySqlException innerException = ex.InnerException as MySqlException;
    					if (innerException != null && innerException.Number == 1062)
							message("DUPLICATE KEY WARNING: ", ConsoleColor.Yellow, ex);
    					else
        					exit(ex);
					}
					catch (Exception e)
					{
						exit(e);
					}

				} while (save_failed);
				return null;
			}
		}

		private void exit(Exception e) {
			message("DATABASE ERROR: ", ConsoleColor.Red, e);
			Environment.Exit(1);
		}

		private void message(string message, ConsoleColor color, Exception e) {
			Logger.LogStatus(color, message + ((e.InnerException == null) ? e.ToString() : e.InnerException.ToString()));
		}
    }
}