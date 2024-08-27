using Microsoft.EntityFrameworkCore;
using PMSModels.Models;

namespace PasswordManagement.Data
{
    public class AccountServices
    {
        #region Variables
        private AccountContext dbContext;
        private readonly ILogger<AccountServices> logger;

        #endregion

        #region Constructor
        public AccountServices(AccountContext accountDBContext , ILogger<AccountServices> plogger)
        {
            dbContext = accountDBContext;
            this.logger = plogger;
        }

        #endregion

        #region Cards CRUD

        public async Task<List<MstCard>> GetAllCards()
        {
            var Collection = await (from a in dbContext.MstCards
                              where a.IsDeleted == false
                              orderby a.CardName ascending
                              select a).ToListAsync();
            return Collection;
        }

        public async Task<MstCard> GetCard(int Id)
        {
            var record = await (from a in dbContext.MstCards
                          where a.Id == Id
                          select a).FirstOrDefaultAsync();
            if (record is not null)
                return record;
            else
            {
                return null;
            }
        }

        public async Task<MstCard> AddCard(MstCard record)
        {
            dbContext.MstCards.Add(record);
            await dbContext.SaveChangesAsync();
            return record;
        }

        public async Task<MstCard> UpdateCard(MstCard record)
        {
            var IfExist = (from a in dbContext.MstCards where a.Id == record.Id select a).FirstOrDefault();
            if(IfExist is not null)
            {
                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
                return record;
            }
            else
            {
                record = null;
                return record;
            }
        }

        public async Task DeleteCard(MstCard record)
        {
            var IfExist = (from a in dbContext.MstCards where a.Id == record.Id select a).FirstOrDefault();
            if(IfExist is not null)
            {
                record.IsDeleted = true;
                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region Settings CRUD

        public async Task<MstSetting> GetSettings()
        {
            var oRecord = await  (from a in dbContext.MstSettings
                           select a).FirstOrDefaultAsync();
            return oRecord;
        }

        public async Task<MstSetting> AddSetting(MstSetting record)
        {
            if(record.Id == 0)
            {
                dbContext.MstSettings.Add(record);
                await dbContext.SaveChangesAsync();
                return record;
            }
            else
            {
                var IfExist = (from a in dbContext.MstSettings where a.Id == record.Id select a).FirstOrDefault();
                if (IfExist is not null)
                {
                    dbContext.Update(record);
                    await dbContext.SaveChangesAsync();
                    return record;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task UpdateSetting(MstSetting record)
        {
            var IfExist = (from a in dbContext.MstSettings where a.Id == record.Id select a).FirstOrDefault();
            if (IfExist is not null)
            {
                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteSetting(MstSetting record)
        {
            var IfExist = (from a in dbContext.MstSettings where a.Id == record.Id select a).FirstOrDefault();
            if (IfExist is not null)
            {

                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region User CRUD

        public async Task<List<MstUser>> GetAllUsers()
        {
            return await dbContext.MstUsers.ToListAsync();
        }

        public async Task<MstUser> CheckUser(MstUser record)
        {
            try
            {
                var IfExists = await (from a in dbContext.MstUsers where a.UserCode == record.UserCode && a.Password == record.Password select a).FirstOrDefaultAsync();
                if (IfExists is not null)
                {
                    return IfExists;
                }
                else
                {
                    MstUser user = null;
                    return user;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Checkuser exception " + ex.Message);
                MstUser user = null;
                return user;
            }
            
        }

        public async Task AddUser(MstUser record)
        {
            dbContext.MstUsers.Add(record);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateUser(MstUser record)
        {
            var IfExist = (from a in dbContext.MstUsers where a.Id == record.Id select a).FirstOrDefault();
            if (IfExist is not null)
            {
                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteUser(MstUser record)
        {
            var IfExist = (from a in dbContext.MstSettings where a.Id == record.Id select a).FirstOrDefault();
            if (IfExist is not null)
            {

                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
            }
        }

        #endregion
    }
}
