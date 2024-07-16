using Develoop.Forms.Blazor.Server.Models;
using Develoop.Forms.Module.BusinessObjects.Moduli;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Develoop.Forms.Blazor.Server.Controllers
{
    [Route("/api")]
    [ApiController]
    [Authorize]
    public class ModuliController : ControllerBase
    {
        internal IConfiguration _configuration;
        internal INonSecuredObjectSpaceFactory _nonSecuredObjectSpaceFactory;
        internal IObjectSpaceFactory _objectSpaceFactory;

        //private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ModuliController(
            IObjectSpaceFactory objectSpaceFactory,
            INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory,
            IConfiguration configuration)
        {
            _objectSpaceFactory = objectSpaceFactory;
            _nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
            _configuration = configuration;
        }

        [HttpGet("ElencoModuli")]

        public ActionResult ElencoModuli()
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<Modulo>())
                {
                    var listaModuli = objectSpace.GetObjectsQuery<Modulo>().ToList();

                    return Ok(listaModuli);
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("ElencoCategorieModuli")]

        public ActionResult ElencoCategorieModuli()
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<CategoriaModulo>())
                {
                    var listaCategorieModuli = objectSpace.GetObjectsQuery<CategoriaModulo>().ToList();

                    return Ok(listaCategorieModuli);
                }
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("RecuperaModulo/{OidModulo}")]
        public ActionResult RecuperaModulo(string OidModulo)
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<Modulo>())
                {
                    var modulo = objectSpace.GetObjectByKey<Modulo>(new Guid(OidModulo));

                    return Ok(modulo);
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("CreaModulo")]
        public ActionResult CreaModulo(string NomeModulo)
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<Modulo>())
                {
                    var modulo = objectSpace.CreateObject<Modulo>();
                    modulo.Nome = NomeModulo;
                    modulo.Save();
                    objectSpace.CommitChanges();

                    return Ok(modulo);
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("RinominaModulo")]
        public ActionResult RinominaModulo(string OidModulo, string NuovoNomeModulo)
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<Modulo>())
                {
                    var modulo = objectSpace.GetObjectByKey<Modulo>(new Guid(OidModulo));
                    modulo.Nome = NuovoNomeModulo;
                    modulo.Save();
                    objectSpace.CommitChanges();

                    return Ok();
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("SalvaModelloModulo")]
        public ActionResult SalvaModelloModulo([FromBody] ChangeSurveyModel model)
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<Modulo>())
                {
                    var modulo = objectSpace.GetObjectByKey<Modulo>(new Guid(model.id));
                    modulo.Json = model.text;
                    modulo.Save();
                    objectSpace.CommitChanges();

                    return Ok(modulo);
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("EliminaModulo")]
        public ActionResult EliminaModulo(string OidModulo)
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<Modulo>())
                {
                    var modulo = objectSpace.GetObjectByKey<Modulo>(new Guid(OidModulo));
                    modulo.Delete();
                    objectSpace.CommitChanges();

                    return Ok(new { id = OidModulo });
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("SalvaRisultatiModulo")]
        public ActionResult SalvaRisultatiModulo([FromBody] PostSurveyResultModel model)
        {
            try
            {
                using(IObjectSpace objectSpace = _nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace<RisultatiModulo>(
                    ))
                {
                    var risultatiModulo = objectSpace.CreateObject<RisultatiModulo>();
                    risultatiModulo.surveyId = model.postId;
                    risultatiModulo.Data = model.surveyResultText;
                    risultatiModulo.Save();
                    objectSpace.CommitChanges();

                    return Ok(risultatiModulo);
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("RecuperaRisultatiModulo")]
        public ActionResult RecuperaRisultatiModulo(string OidModulo)
        {
            try
            {
                using(IObjectSpace objectSpace = _objectSpaceFactory.CreateObjectSpace<RisultatiModulo>(
                    ))
                {
                    var listaRisultatiModulo = objectSpace.GetObjectsQuery<RisultatiModulo>()
                        .Where(r => r.surveyId == OidModulo)
                        .ToList();

                    if(listaRisultatiModulo != null)
                    {
                        return Ok(new { data = listaRisultatiModulo });
                    } else
                    {
                        return BadRequest();
                    }
                }
            } catch(Exception)
            {
                return BadRequest();
            }
        }

        // // GET api/values/5
        // [HttpGet("{id}")]
        // public string Get(int id)
        // {
        //     return "value";
        // }

        // // POST api/values
        // [HttpPost]
        // public void Post([FromBody]string value)
        // {
        // }

        // // PUT api/values/5
        // [HttpPut("{id}")]
        // public void Put(int id, [FromBody]string value)
        // {
        // }

        // // DELETE api/values/5
        // [HttpDelete("{id}")]
        // public void Delete(int id)
        // {
        // }
    }
}
