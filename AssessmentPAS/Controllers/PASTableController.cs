using AssessmentPAS.Data;
using AssessmentPAS.Dto;
using AssessmentPAS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AssessmentPAS.Controllers
{
    [ApiController]
    [Route("api/")]
    public class PASTableController : Controller
    {
        private readonly PASDbContext pasDbContext;
        public PASTableController(PASDbContext pasDbContext )
        {
            this.pasDbContext = pasDbContext;
        }
        //Read operations

        //Get form using formid
        [HttpGet]
        [Route("Form/id/{id:guid}")]
        public async Task<IActionResult> GetFormById([FromRoute] Guid id)
        {
            var form = await pasDbContext.Forms.FirstOrDefaultAsync(x => x.Id == id);
            if(form == null) {
                return NotFound("Item Not Found");
            }
            return Ok(form);
        }
        //Get forms of a type

        [HttpGet]
        [Route("Form/Type/{type}")]
        public async Task<IActionResult> GetFormByType([FromRoute] string type)
        {
            try
            {
                var forms = await pasDbContext.Forms.Where(x => x.Type == type).ToListAsync();
                if (forms.Count == 0)
                {
                    return NotFound($"To Forms with type '{type}' was found");
                }
                return Ok(forms);
            }
            catch 
            {
                return StatusCode(500,$"Error Occured While Getting Form of Type '{type}'");
            }
        }

        //Get  forms of a table along with its table name using table id
        [HttpGet]
        [Route("Forms/TableId/{TableId:Guid}")]
        public async Task<ActionResult<List<Aotable>>> GetAllFormsByTableId([FromRoute] Guid TableId)
        {
            try
            {
                var table = await pasDbContext.Aotables.FirstOrDefaultAsync(x => x.Id == TableId);
                var forms = await pasDbContext.Forms.Include("Table").Where(x => x.TableId == TableId).ToListAsync();
                if (table != null && forms.Any())
                {
                    var re = new
                    {
                        forms,
                        tablename = table.Name
                    };
                    return Ok(re);
                }
                return BadRequest("No Table Id found for Form Table");
            }
            catch
            {
                return StatusCode(500);
            }

        }
        //Get all records from Form table associated to the TableName (AOTable) passed as parameter
        [HttpGet]
        [Route("Form/TableName/{TableName}")]
        public async Task<IActionResult> GetAllFormsByTableName([FromRoute] string TableName)
        {
            try
            {
                var table = await pasDbContext.Aotables.FirstOrDefaultAsync(x => x.Name ==TableName);
                if(table==null)
                {
                    return NotFound($"Table - '{TableName}' not found");
                }
                var forms = await pasDbContext.Forms.Where(x=>x.TableId==table.Id).ToListAsync();
                if (forms.Any())
                {
                    return Ok(forms);
                }
                return NotFound($"Table - '{TableName}({table.Id})'has no forms");
                
            }
            catch
            {
                return StatusCode(500);
            }
        }
        //Add record to form table using table name
        [HttpPost]
        [Route("Form/Add/{TableName}")]
        public async Task<IActionResult> AddFormByTableName([FromRoute] string TableName, [FromBody] Form newform)
        {

            try
            {
                var table = await pasDbContext.Aotables.FirstOrDefaultAsync(x => x.Name == TableName);
                if (table != null)
                {
                    newform.Id = Guid.NewGuid();
                    newform.TableId = table.Id;
                    await pasDbContext.Forms.AddAsync(newform);
                    await pasDbContext.SaveChangesAsync();
                    //return CreatedAtAction(nameof(AddFormByTableName), new { id = newform.Id }, newform);
                    return Ok(newform);
                }
                return NotFound($"No Mathcing found for TableName '{TableName}'");
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        //Edit a record in Form table by passing Form Name as the parameter
        [HttpPatch]
        [Route("Form/Update/{FormName}")]
        public async Task<IActionResult> UpdateFormByName([FromRoute] string FormName, [FromBody] EditFormDto editform)
        {
            try
            {
                var oldform = await pasDbContext.Forms.FirstOrDefaultAsync(x => x.Name == FormName);
                if (oldform != null)
                {

                    oldform.Name = editform.Name ?? oldform.Name;
                    oldform.Sequence = editform.Sequence ?? oldform.Sequence;
                    oldform.Comment = editform.Comment ?? oldform.Comment;
                    oldform.TabResourceName = editform.TabResourceName ?? oldform.TabResourceName;
                    oldform.SubSequence = editform.SubSequence ?? oldform.SubSequence;
                    oldform.MinOccurs = editform.MinOccurs ?? oldform.MinOccurs;
                    oldform.MaxOccurs = editform.MaxOccurs ?? oldform.MaxOccurs;
                    oldform.BtnCndAdd = editform.BtnCndAdd ?? oldform.BtnCndAdd;
                    oldform.BtnCndCopy = editform.BtnCndCopy ?? oldform.BtnCndCopy;
                    oldform.BtnCndAdd = editform.BtnCndAdd ?? oldform.BtnCndAdd;
                    oldform.Condition = editform.Condition ?? oldform.Condition;
                    oldform.AddChangeDeleteFlag = editform.AddChangeDeleteFlag ?? oldform.AddChangeDeleteFlag;
                    await pasDbContext.SaveChangesAsync();
                    return Ok(oldform);
                }
                return NotFound($"Form {FormName} not found");
            }
            catch
            {
                return StatusCode(500,"Error occured");
            }

        }
        [HttpDelete]
        [Route("Form/Delete/{id:guid}")]
        public async Task<IActionResult> DeleteFormById([FromRoute] Guid id)
        {
            try
            {
                var form = await pasDbContext.Forms.FindAsync(id);
                if (form != null)
                {
                    pasDbContext.Remove(form);
                    await pasDbContext.SaveChangesAsync();
                    return Ok(form);
                }
                return NotFound("Form Not Found");
            }
            catch
            {
                return StatusCode(500, "Error Occured");
            }
        }


    }
}
