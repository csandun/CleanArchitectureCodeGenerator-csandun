using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.CodeGenerator.Models
{
	public class TemplateModel
	{
		public string Id  { get; set; }
		public string Action { get; set; }
		public string TemplatePath { get; set; }
		public string FilePath { get; set; }
		public string Category { get; set; }
	}

	public static class TemplateModelExtensions
	{

		public static List<TemplateModel> Templates = new List<TemplateModel>(){
			// create
			new TemplateModel(){ 
				Id = "CreateHandler",
				Action = "Create",
				FilePath = "$NAME/Create$NAMECommandHandler.cs",
				TemplatePath = "",
				Category = "Command"
			},
			new TemplateModel(){
				Id = "CreateCommand",
				Action = "Create",
				FilePath = "$NAME/Create$NAMECommand.cs",
				TemplatePath = "",
				Category = "Command"
			},
			new TemplateModel(){
				Id = "CreateValidator",
				Action = "Create",
				FilePath = "$NAME/Create$NAMECommandValidator.cs",
				TemplatePath = "",
				Category = "Command"
			},

			// update
			new TemplateModel(){
				Id = "UpdateHandler",
				Action = "Update",
				FilePath = "$NAME/Update$NAMECommandHandler.cs",
				TemplatePath = "",
				Category = "Command"
			},
			new TemplateModel(){
				Id = "UdpateCommand",
				Action = "Update",
				FilePath = "$NAME/Update$NAMECommand.cs",
				TemplatePath = "",
				Category = "Command"
			},
			new TemplateModel(){
				Id = "UdpateValidator",
				Action = "Update",
				FilePath = "$NAME/Update$NAMECommandValidator.cs",
				TemplatePath = "",
				Category = "Command"
			},

			// delete
			new TemplateModel(){
				Id = "DeleteHandler",
				Action = "Delete",
				FilePath = "$NAME/Delete$NAMECommandHandler.cs",
				TemplatePath = "",
				Category = "Command"
			},
			new TemplateModel(){
				Id = "DeleteCommand",
				Action = "Delete",
				FilePath = "$NAME/Delete$NAMECommand.cs",
				TemplatePath = "",
				Category = "Command"
			},


			// get all
			new TemplateModel(){
				Id = "GetAllHandler",
				Action = "GetAll",
				FilePath = "$NAME/GetAll$NAME_OF_PLURALQueryHandler.cs",
				TemplatePath = "",
				Category = "Query"
			},
			new TemplateModel(){
				Id = "GetAllQuery",
				Action = "GetAll",
				FilePath = "$NAME/GetAll$NAME_OF_PLURALQuery.cs",
				TemplatePath = "",
				Category = "Query"
			},

			// get by id
			new TemplateModel(){
				Id = "GetByIdHandler",
				Action = "GetById",
				FilePath = "$NAME/Get$NAMEByIdQueryHandler.cs",
				TemplatePath = "",
				Category = "Query"
			},
			new TemplateModel(){
				Id = "GetAllQuery",
				Action = "GetAll",
				FilePath = "$NAME/Get$NAMEByIdQuery.cs",
				TemplatePath = "",
				Category = "Query"
			},

			// get with pagination
			new TemplateModel(){
				Id = "GetAllWithPaginationHandler",
				Action = "GetById",
				FilePath = "$NAME/Get$NAME_OF_PLURALQueryHandler.cs",
				TemplatePath = "",
				Category = "Query"
			},
			new TemplateModel(){
				Id = "GetAllWithPaginationQuery",
				Action = "GetAll",
				FilePath = "$NAME/Get$NAME_OF_PLURALQuery.cs",
				TemplatePath = "",
				Category = "Query"
			},

		};
	
	
	}
}
