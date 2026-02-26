using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using isc.time.report.be.application.Interfaces.Repository.Clients;
using isc.time.report.be.application.Interfaces.Repository.Leaders;
using isc.time.report.be.application.Interfaces.Repository.Projects;
using isc.time.report.be.application.Interfaces.Service.Projects;
using isc.time.report.be.domain.Entity.Catalogs;
using isc.time.report.be.domain.Entity.Employees;
using isc.time.report.be.domain.Entity.Projects;
using isc.time.report.be.domain.Entity.Shared;
using isc.time.report.be.domain.Exceptions;
using isc.time.report.be.domain.Models.Request.Projects;
using isc.time.report.be.domain.Models.Response.Employees;
using isc.time.report.be.domain.Models.Response.Persons;
using isc.time.report.be.domain.Models.Response.Projects;

namespace isc.time.report.be.application.Services.Projects
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository projectRepository;
        private readonly IMapper _mapper;
        private readonly IClientRepository _clientRepository;
        private readonly ILeaderRepository _leaderRepository;
        public ProjectService(IProjectRepository projectRepository, IMapper mapper, IClientRepository clientRepository, ILeaderRepository leaderRepository)
        {
            this.projectRepository = projectRepository;
            _mapper = mapper;
            _clientRepository = clientRepository;
            _leaderRepository = leaderRepository;
        }

        public async Task<PagedResult<GetAllProjectsResponse>> GetAllProjectsPaginated(PaginationParams paginationParams, string? search)
        {
            var result = await projectRepository.GetAllProjectsPaginatedAsync(paginationParams, search);

            var responseItems = _mapper.Map<List<GetAllProjectsResponse>>(result.Items);

            return new PagedResult<GetAllProjectsResponse>
            {
                Items = responseItems,
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<PagedResult<GetAllProjectsResponse>> GetAllProjectsByEmployeeIDPaginated(
            PaginationParams paginationParams,
            string? search,
            int employeeId,
            bool active)
        {
            if (active == true)
            {
                var result = await projectRepository.GetAssignedProjectsForEmployeeActiveAsync(paginationParams, search, employeeId);

                var responseItems = _mapper.Map<List<GetAllProjectsResponse>>(result.Items);

                for (int i = 0; i < result.Items.Count; i++)
                {
                    var project = result.Items[i];
                    var response = responseItems[i];
                    if (project.Leader != null)
                    {
                        response.LeaderID = project.LeaderID;
                        response.Leader = new Lider
                        {
                            Id = project.Leader.Id,
                            FirstName = project.Leader.FirstName,
                            LastName = project.Leader.LastName,
                            Email = project.Leader.Email
                        };
                    }
                }

                return new PagedResult<GetAllProjectsResponse>
                {
                    Items = responseItems,
                    TotalItems = result.TotalItems,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
            }
            else
            {
                var result = await projectRepository.GetAssignedProjectsForEmployeeAsync(paginationParams, search, employeeId);

                var responseItems = _mapper.Map<List<GetAllProjectsResponse>>(result.Items);

                for (int i = 0; i < result.Items.Count; i++)
                {
                    var project = result.Items[i];
                    var response = responseItems[i];
                    if (project.Leader != null)
                    {
                        response.LeaderID = project.LeaderID;
                        response.Leader = new Lider
                        {
                            Id = project.Leader.Id,
                            FirstName = project.Leader.FirstName,
                            LastName = project.Leader.LastName,
                            Email = project.Leader.Email
                        };
                    }
                }

                return new PagedResult<GetAllProjectsResponse>
                {
                    Items = responseItems,
                    TotalItems = result.TotalItems,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };
            }

        }

        public async Task<GetProjectByIDResponse> GetProjectByID(int projectID)
        {
            var project = await projectRepository.GetProjectByIDAsync(projectID);

            if (project == null)
            {
                return null;
            }

            return _mapper.Map<GetProjectByIDResponse>(project);
        }


        public async Task<CreateProjectResponse> CreateProject(CreateProjectRequest projectRequest)
        {
            var projectEntity = _mapper.Map<Project>(projectRequest);

            if (projectRequest.LeaderID.HasValue)
            {
                var leader = await _leaderRepository.GetLeaderByIDAsync(projectRequest.LeaderID.Value);
                if (leader == null)
                {
                    throw new ClientFaultException("El líder especificado no existe.", 404);
                }
            }
            projectEntity.LeaderID = projectRequest.LeaderID;

            var projectNew = await projectRepository.CreateProject(projectEntity);

            if (projectNew.StartDate > projectNew.EndDate)
            {
                throw new ClientFaultException("No puede ingresar una fecha de Fin anterior a la fecha de Inicio.", 401);
            }

            return _mapper.Map<CreateProjectResponse>(projectNew);
        }

        public async Task<UpdateProjectResponse> UpdateProject(int projectId, UpdateProjectRequest projectParaUpdate)
        {
            var projectGet = await projectRepository.GetProjectByIDAsync(projectId);

            if (projectGet == null)
            {
                throw new ClientFaultException("No existe el proyecto", 401);
            }

            if (projectParaUpdate.LeaderID.HasValue && projectGet.LeaderID != projectParaUpdate.LeaderID)
            {
                var leader = await _leaderRepository.GetLeaderByIDAsync(projectParaUpdate.LeaderID.Value);
                if (leader == null)
                {
                    throw new ClientFaultException("El líder especificado no existe.", 404);
                }
            }

            projectGet.ClientID = projectParaUpdate.ClientID;
            projectGet.ProjectStatusID = projectParaUpdate.ProjectStatusID;
            projectGet.ProjectTypeID = projectParaUpdate.ProjectTypeID;
            projectGet.Code = projectParaUpdate.Code;
            projectGet.Name = projectParaUpdate.Name;
            projectGet.Description = projectParaUpdate.Description;
            projectGet.StartDate = projectParaUpdate.StartDate;
            projectGet.EndDate = projectParaUpdate.EndDate;
            projectGet.ActualStartDate = projectParaUpdate.ActualStartDate;
            projectGet.ActualEndDate = projectParaUpdate.ActualEndDate;
            projectGet.Budget = projectParaUpdate.Budget;
            projectGet.Hours = projectParaUpdate.Hours;
            projectGet.WaitingStartDate = projectParaUpdate.WaitingStartDate;
            projectGet.WaitingEndDate = projectParaUpdate.WaitingEndDate;
            projectGet.Observation = projectParaUpdate.Observation;
            projectGet.LeaderID = projectParaUpdate.LeaderID; 

            if (projectGet.StartDate > projectGet.EndDate)
            {
                throw new ClientFaultException("No puede ingresar una fecha de Fin anterior a la fecha de Anterior.", 401);
            }

            var projectUpdated = await projectRepository.UpdateProjectAsync(projectGet);

            return _mapper.Map<UpdateProjectResponse>(projectUpdated);
        }

        public async Task<ActiveInactiveProjectResponse> InactiveProject(int projectId)
        {

            var projectInactive = await projectRepository.InactivateProjectAsync(projectId);

            return _mapper.Map<ActiveInactiveProjectResponse>(projectInactive);
        }

        public async Task<ActiveInactiveProjectResponse> ActiveProject(int ProjectId)
        {

            var projectActive = await projectRepository.ActivateProjectAsync(ProjectId);

            return _mapper.Map<ActiveInactiveProjectResponse>(projectActive);
        }

        public async Task AssignEmployeesToProject(AssignEmployeesToProjectRequest request)
        {
            Project project = await projectRepository.GetProjectByIDAsync(request.ProjectID);

            if (project == null)
                throw new ArgumentException($"No se encontró el proyecto con ID {request.ProjectID}");

            DateTime now = DateTime.UtcNow;


            foreach (var dto in request.EmployeeProjectMiddle)
            {
                bool tieneEmpleado = dto.EmployeeId.HasValue;
                bool tieneProveedor = dto.SupplierID.HasValue;

                if (tieneEmpleado == tieneProveedor)
                {
                    throw new ArgumentException(
                        $"Cada asignación debe tener solo EmployeeId o solo SupplierID. " +
                        $"DTO con EmployeeId={dto.EmployeeId} SupplierID={dto.SupplierID} no válido."
                    );
                }
            }

            var existingAssignments = await projectRepository.GetByProjectEmployeeIDAsync(request.ProjectID);
            var finalList = new List<EmployeeProject>();

            foreach (var dto in request.EmployeeProjectMiddle)
            {
                var match = existingAssignments.FirstOrDefault(ep =>
                    ep.EmployeeID == dto.EmployeeId &&
                    ep.SupplierID == dto.SupplierID
                );

                if (match == null)
                {
                    finalList.Add(new EmployeeProject
                    {
                        ProjectID = request.ProjectID,
                        EmployeeID = dto.EmployeeId,
                        SupplierID = dto.SupplierID,
                        AssignedRole = dto.AssignedRole,
                        CostPerHour = dto.CostPerHour,
                        AllocatedHours = dto.AllocatedHours,
                        Status = true,
                        AssignmentDate = now,
                        CreationDate = now,
                        CreationUser = "SYSTEM"
                    });
                }
                else
                {
                    if (!match.Status)
                    {
                        match.Status = true;
                        match.ModificationDate = now;
                        match.ModificationUser = "SYSTEM";
                    }

                    match.AssignedRole = dto.AssignedRole;
                    match.CostPerHour = dto.CostPerHour;
                    match.AllocatedHours = dto.AllocatedHours;

                    finalList.Add(match);
                }
            }

            foreach (var ep in existingAssignments)
            {
                bool sigueEnRequest = request.EmployeeProjectMiddle.Any(dto =>
                    dto.EmployeeId == ep.EmployeeID &&
                    dto.SupplierID == ep.SupplierID
                );

                if (!sigueEnRequest && ep.Status)
                {
                    ep.Status = false;
                    ep.ModificationDate = now;
                    ep.ModificationUser = "SYSTEM";
                    finalList.Add(ep);
                }
            }

            await projectRepository.SaveAssignmentsAsync(finalList);
        }


        public async Task<GetProjectDetailByIDResponse?> GetProjectDetailByID(int projectID)
        {
            var project = await projectRepository.GetProjectDetailByIDAsync(projectID);

            if (project == null)
                return null;

            var response = new GetProjectDetailByIDResponse
            {
                Id = project.Id,
                ClientID = project.ClientID,
                ProjectStatusID = project.ProjectStatusID,
                Code = project.Code,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ActualStartDate = project.ActualStartDate,
                ActualEndDate = project.ActualEndDate,
                Budget = project.Budget,
                WaitingEndDate = project.WaitingEndDate,
                WaitingStartDate = project.WaitingStartDate,

                Observation = project.Observation,
                LeaderID = project.LeaderID,
                Leader = project.Leader != null ? new isc.time.report.be.domain.Models.Response.Projects.Lider
                {
                    Id = project.Leader.Id,
                    FirstName = project.Leader.FirstName,
                    LastName = project.Leader.LastName,
                    Email = project.Leader.Email
                } : null,

                EmployeeProjects = project.EmployeeProject.Select(ep => new GetEmployeeProjectResponse
                {
                    Id = ep.Id,
                    EmployeeID = ep.EmployeeID,
                    SupplierID = ep.SupplierID,
                    AssignedRole = ep.AssignedRole,
                    CostPerHour = ep.CostPerHour,
                    AllocatedHours = ep.AllocatedHours,
                    ProjectID = ep.ProjectID,
                    Status = ep.Status
                }).ToList(),

                EmployeesPersonInfo = project.EmployeeProject
                    .Where(ep => ep.Employee != null)
                    .Select(ep => ep.Employee)
                    .Distinct()
                    .Select(e => new GetEmployeesPersonInfoResponse
                    {
                        Id = e.Id,
                        PersonID = e.PersonID,
                        EmployeeCode = e.EmployeeCode,
                        IdentificationNumber = e.Person.IdentificationNumber,
                        FirstName = e.Person.FirstName,
                        LastName = e.Person.LastName,
                        Status = e.Status
                    }).ToList()
            };

            return response;
        }

        public async Task<List<GetProjectsByEmployeeIDResponse>> GetProjectsByEmployeeIdAsync(int employeeId)
        {
            var projects = await projectRepository.GetProjectsByEmployeeIdAsync(employeeId);
            return _mapper.Map<List<GetProjectsByEmployeeIDResponse>>(projects);
        }
        public async Task<List<CreateDtoToExcelProject>> GetProjectsForExcelAsync()
        {
            var projects = await projectRepository.GetAllProjectsAsync();
            if (projects == null || !projects.Any())
                return new List<CreateDtoToExcelProject>();

            var projectIds = projects.Select(p => p.Id).ToList();

            var leaders = await _leaderRepository.GetActiveLeadersByProjectIdsAsync(projectIds);

            var clientIds = projects.Select(p => p.ClientID).Distinct().ToList();
            var clients = await _clientRepository.GetListOfClientsByIdsAsync(clientIds);

            var result = projects.Select((p, index) =>
            {
                var currentLeader = p.Leader;
                var leaderList = new List<LiderData>();

                if (currentLeader != null)
                {
                    leaderList.Add(new LiderData
                    {
                        Id = currentLeader.Id,
                        GetPersonResponse = new GetPersonResponse
                        {
                            FirstName = currentLeader.FirstName,
                            LastName = currentLeader.LastName,
                            Email = currentLeader.Email,
                            Phone = currentLeader.Phone
                        }
                    });
                }

                var projectClients = clients
                    .Where(c => c.Id == p.ClientID)
                    .Select(c => new ClientData
                    {
                        TradeName = c.TradeName

                    })
                    .ToList();

                return new CreateDtoToExcelProject
                {
                    Id = p.Id,
                    ClientID = p.ClientID,
                    ProjectType = new ProjectType { TypeName = p.ProjectType?.TypeName },
                    ProjectStatus = new ProjectStatus { StatusName = p.ProjectStatus?.StatusName },
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ActualStartDate = p.ActualStartDate,
                    ActualEndDate = p.ActualEndDate,
                    Budget = p.Budget,
                    Hours = p.Hours,
                    Status = p.Status,
                    WaitingStartDate = p.WaitingStartDate,
                    WaitingEndDate = p.WaitingEndDate,
                    Observation = p.Observation,
                    LiderData = leaderList,
                    ClientData = projectClients,

                };
            }).ToList();

            return result;
        }

        public async Task<byte[]> GenerateProjectsExcelAsync()
        {
            var projects = await GetProjectsForExcelAsync();

            if (projects == null || !projects.Any())
                return Array.Empty<byte>();

            using (var memoryStream = new MemoryStream())
            {
                using (var spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylesPart.Stylesheet = CreateStylesheet();
                    stylesPart.Stylesheet.Save();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    //Definir anchos de columnas
                    var columns = new Columns();
                    columns.Append(CreateColumn(1, 1, 6)); // NRO
                    columns.Append(CreateColumn(2, 2, CalculateColumnWidth(new[] { "Código Proyecto" }.Concat(projects.Select(p => p.Code)))));
                    columns.Append(CreateColumn(3, 3, CalculateColumnWidth(new[] { "Proyecto" }.Concat(projects.Select(p => p.Name)))));
                    columns.Append(CreateColumn(4, 4, CalculateColumnWidth(new[] { "Líder" }.Concat(projects.Select(p =>
                        p.LiderData?.FirstOrDefault() != null
                            ? $"{p.LiderData.First().GetPersonResponse.FirstName} {p.LiderData.First().GetPersonResponse.LastName}"
                            : string.Empty
                    )))));
                    columns.Append(CreateColumn(5, 5, CalculateColumnWidth(new[] { "Cliente" }.Concat(projects.Select(p => p.ClientData?.FirstOrDefault()?.TradeName ?? "")))));
                    columns.Append(CreateColumn(6, 6, CalculateColumnWidth(new[] { "Estado Proyecto" }.Concat(projects.Select(p => p.ProjectStatus?.StatusName ?? "")))));
                    columns.Append(CreateColumn(7, 7, CalculateColumnWidth(new[] { "Tipo Proyecto" }.Concat(projects.Select(p => p.ProjectType?.TypeName ?? "")))));
                    columns.Append(CreateColumn(8, 8, 20));  // Fecha inicio
                    columns.Append(CreateColumn(9, 9, 22));  // Fecha fin estimada
                    columns.Append(CreateColumn(10, 10, 22)); // Fecha fin real
                    columns.Append(CreateColumn(11, 11, 18)); // Presupuesto
                    columns.Append(CreateColumn(12, 12, 15)); // Horas
                    columns.Append(CreateColumn(13, 13, 22)); // Fecha inicio espera
                    columns.Append(CreateColumn(14, 14, 22)); // Fecha fin espera
                    columns.Append(CreateColumn(15, 15, CalculateColumnWidth(new[] { "Observaciones" }.Concat(projects.Select(p => p.Observation ?? "")))));

                    // SheetData y fila 1 - título
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.Append(columns); // Columnas primero
                    worksheetPart.Worksheet.Append(sheetData);

                    var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Proyectos"
                    };
                    sheets.Append(sheet);

                    // MergeCells (después de sheetData pero antes de filas)
                    var mergeCells = new MergeCells();
                    worksheetPart.Worksheet.InsertAfter(mergeCells, sheetData);
                    mergeCells.Append(new MergeCell() { Reference = new StringValue("A1:O1") });

                    var row1 = new Row { RowIndex = 1, Height = 35, CustomHeight = true };
                    row1.Append(CreateCell("PROYECTOS", styleIndex: 2)); // Estilo 2 = título
                    sheetData.Append(row1);

                    // Cabecera
                    var headerRow = new Row();
                    headerRow.Append(
                        CreateCell("NRO", 1),
                        CreateCell("CODIGO DEL PROYECTO", 1),
                        CreateCell("PROYECTO", 1),
                        CreateCell("LIDER", 1),
                        CreateCell("CLIENTE", 1),
                        CreateCell("ESTADO DEL PROYECTO", 1),
                        CreateCell("TIPO DE PROYECTO", 1),
                        CreateCell("FECHA DE INICIO", 1),
                        CreateCell("FECHA DE FIN ESTIMADA", 1),
                        CreateCell("FECHA FIN REAL", 1),
                        CreateCell("PRESUPUESTO", 1),
                        CreateCell("HORAS", 1),
                        CreateCell("FECHA INICIO ESPERA", 1),
                        CreateCell("FECHA FIN ESPERA", 1),
                        CreateCell("OBSERVACIONES", 1)
                    );
                    sheetData.AppendChild(headerRow);

                    // Filas con los datos
                    int nro = 1;
                    foreach (var item in projects)
                    {
                        var row = new Row();
                        row.Append(
                            CreateCell(nro.ToString(), 3),
                            CreateCell(item.Code, 3),
                            CreateCell(item.Name, 3),
                            CreateCell(item.LiderData?.FirstOrDefault() != null
                                ? $"{item.LiderData.First().GetPersonResponse.FirstName} {item.LiderData.First().GetPersonResponse.LastName}"
                                : string.Empty, 3),
                            CreateCell(item.ClientData?.FirstOrDefault()?.TradeName ?? string.Empty, 3),
                            CreateCell(item.ProjectStatus?.StatusName ?? "", 3),
                            CreateCell(item.ProjectType?.TypeName ?? "", 3),
                            CreateCell(item.StartDate?.ToShortDateString() ?? string.Empty, 3),
                            CreateCell(item.EndDate?.ToShortDateString() ?? string.Empty, 3),
                            CreateCell(item.ActualEndDate?.ToShortDateString() ?? string.Empty, 3),
                            CreateCell(item.Budget?.ToString("N2") ?? "0", 3),
                            CreateCell(item.Hours.ToString(), 3),
                            CreateCell(item.WaitingStartDate?.ToShortDateString() ?? string.Empty, 3),
                            CreateCell(item.WaitingEndDate?.ToShortDateString() ?? string.Empty, 3),
                            CreateCell(item.Observation ?? string.Empty, 3)
                        );
                        sheetData.AppendChild(row);
                        nro++;
                    }
                }

                return memoryStream.ToArray();
            }
        }

        // Helpers
        private Cell CreateCell(string value, uint styleIndex = 0)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(value ?? ""),
                StyleIndex = styleIndex
            };
        }

        private Column CreateColumn(uint min, uint max, double width)
        {
            return new Column
            {
                Min = min,
                Max = max,
                Width = width,
                CustomWidth = true
            };
        }

        private double CalculateColumnWidth(IEnumerable<string> values)
        {
            if (!values.Any())
                return 10;

            int maxLength = values.Max(v => v?.Length ?? 0);

            // Aproximación: cada carácter 1.2 unidades en Excel
            return Math.Min(100, maxLength * 1.2);
        }
        private Stylesheet CreateStylesheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font( 
                        new FontSize { Val = 11 },
                        new FontName { Val = "Calibri" }
                    ),
                    new Font(
                        new FontSize { Val = 11 },
                        new Bold(),
                        new FontName { Val = "Calibri" }
                    ),
                    new Font( 
                        new FontSize { Val = 25 },
                        new Bold(),
                        new FontName { Val = "Calibri" }
                    )
                ),

                new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }), // 0 - Default
                    new Fill(new PatternFill { PatternType = PatternValues.Gray125 }), // 1 - Default
                    new Fill(new PatternFill( // 2 - Gris oscuro 25%
                        new ForegroundColor { Rgb = "FFBFBFBF" })
                    { PatternType = PatternValues.Solid })
                ),

                new Borders(
                    new Border(), // 0 - Sin bordes (para título)
                    new Border(   // 1 - Bordes completos (para cabeceras y celdas normales)
                        new LeftBorder { Style = BorderStyleValues.Thin },
                        new RightBorder { Style = BorderStyleValues.Thin },
                        new TopBorder { Style = BorderStyleValues.Thin },
                        new BottomBorder { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),

                new CellFormats(
                    new CellFormat { FontId = 0, FillId = 0, BorderId = 0 }, // 0 - Default

                    new CellFormat // 1 - Cabeceras
                    {
                        FontId = 1,
                        FillId = 2,
                        BorderId = 1,
                        Alignment = new Alignment
                        {
                            Horizontal = HorizontalAlignmentValues.Center,
                            Vertical = VerticalAlignmentValues.Center,
                            WrapText = true
                        },
                        ApplyFont = true,
                        ApplyFill = true,
                        ApplyBorder = true,
                        ApplyAlignment = true
                    },

                    new CellFormat // 2 - Título grande, centrado, sin bordes
                    {
                        FontId = 2,
                        FillId = 0,
                        BorderId = 0,
                        Alignment = new Alignment
                        {
                            Horizontal = HorizontalAlignmentValues.Center,
                            Vertical = VerticalAlignmentValues.Center
                        },
                        ApplyFont = true,
                        ApplyFill = false,
                        ApplyBorder = false,
                        ApplyAlignment = true
                    },

                    new CellFormat // 3 - Celdas normales con bordes Calibri 11
                    {
                        FontId = 0,
                        FillId = 0,
                        BorderId = 1,
                        Alignment = new Alignment
                        {
                            Horizontal = HorizontalAlignmentValues.Left,
                            Vertical = VerticalAlignmentValues.Center,
                            WrapText = true
                        },
                        ApplyFont = true,
                        ApplyFill = false,
                        ApplyBorder = true,
                        ApplyAlignment = true
                    }
                )
            );
        }




    }

}

