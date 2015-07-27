using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;

namespace WanTai.Controller.Configuration
{
    public class RoleInfoController
    {
        public List<RoleInfo> GetAll()
        { 
            using(WanTaiEntities _WanTaiEntities=new WanTaiEntities())
            {
                UserInfo user = _WanTaiEntities.UserInfoes.FirstOrDefault(P=>P.LoginName==SessionInfo.LoginName);
                RoleInfo role=_WanTaiEntities.RoleInfoes.FirstOrDefault(P=>P.RoleName==user.RoleName);
                return _WanTaiEntities.RoleInfoes.Where(P => P.RoleLevel <= role.RoleLevel).OrderBy(P=>P.RoleLevel).ToList();
            }
        }
    }
}
