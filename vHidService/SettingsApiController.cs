using System.IO;
using System.Linq;
using System.Web.Http;
using System.Xml.Serialization;

namespace vHidService
{
    [RoutePrefix("settings")]
    public class SettingsController : ApiController
    {
        public SettingsController()
        {
        }

        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(Context.Settings);
        }

        [Route("")]
        public void Post([FromBody]Settings value)
        {
            Context.Settings = value;
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }

    [RoutePrefix("manualEffects")]
    public class ManualEffectsController : ApiController
    {
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok(Directory.GetFiles("Profiles", "*.xml")
                                     .Select(Path.GetFileNameWithoutExtension)
                                     .ToArray());
        }

        [Route("profile/{file}")]
        public IHttpActionResult Get(string file)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(ManualConditionEffects));
            using (TextReader reader = new StreamReader(@"Profiles\" + file + ".xml"))
            {
                object obj = deserializer.Deserialize(reader);
                return Ok((ManualConditionEffects)obj);
            }
        }

        [Route("profile/{file}")]
        public IHttpActionResult Post(string file, [FromBody]ManualConditionEffects effect)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ManualConditionEffects));
            using (TextWriter writer = new StreamWriter(@"Profiles\" + file + ".xml"))
            {
                serializer.Serialize(writer, effect);
            }
            Context.ManualConditionEffects = effect;
            VhidService.manualFfb.UpdateEffects();
            return Ok();
        }
    }
}