using MainRestApi.Models;
using System;
using System.Web.Http;

namespace MainRestApi.Controllers
{
    public class IsBankController : ApiController
    {
        [HttpGet]
        public IsCepState IsCepStateKodUret()
        {
            IsCepState isCepState = new IsCepState();

            isCepState.state = Guid.NewGuid().ToString();

            return isCepState;
        }

        [HttpGet]
        public IsCepDeeplinkSchema IsCepDeeplinkSchema()
        {
            IsCepDeeplinkSchema isCepDeeplinkSchema = new IsCepDeeplinkSchema();

            isCepDeeplinkSchema.url = "dijitalkasa://scope=iscepregister&state={state}&authorize_code={authorize_code}&tckn={tckn}";

            return isCepDeeplinkSchema;
        }

        [HttpGet]
        public IsCepDeeplinkSchemaEmpty IsCepDeeplinkSchemaEmpty()
        {
            IsCepDeeplinkSchemaEmpty isCepDeeplinkSchemaEmpty = new IsCepDeeplinkSchemaEmpty();

            isCepDeeplinkSchemaEmpty.url = "dijitalkasa://scope=iscepregister";

            return isCepDeeplinkSchemaEmpty;
        }
    }
}