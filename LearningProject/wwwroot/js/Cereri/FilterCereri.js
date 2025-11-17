
        // Call on page load

window.addEventListener('DOMContentLoaded', () => {
        loadFilteredInputs(); 
                            });

    function initializeAutocomplete() {
        $("#searchField").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/GetAutofill',
                    type: 'GET',
                    dataType: 'json',
                    data: { autocompleteText: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item.name, value: item.name };
                        }));
                    },
                    error: function (xhr, status, error) {
                        console.error("AJAX error:", error);
                    }
                });
            },
            minLength: 2,
            select: function (event, ui) {
                event.preventDefault();
                $("#searchField").val(ui.item.value);
                // Reîncarcă tabela folosind search-ul selectat
                loadFilteredInputs(1, null, ui.item.value);
            }
        });
                        }


    function loadFilteredInputs(pageNumber = 1, sort = null, searchStringCerere = '', filter='', nrCrt = null, description = '', creat_de='', sters_de='', data_creare='', data_stergere='') {
                            // Preia valorile curente
                            try {
        filter = document.getElementById("filterSelect")?.value ?? '';
    searchInput = document.getElementById("searchField")?.value ?? '';
    nrCrt = document.getElementById("nrCrtFilter")?.value ?? '';
    description = document.getElementById("descriptionFilter")?.value ?? '';
    creat_de = document.getElementById("createdByFilter")?.value ?? '';
    sters_de = document.getElementById("deletedByFilter")?.value ?? '';
    data_creare = document.getElementById("createdOnFilter")?.value ?? '';
    data_stergere = document.getElementById("deletedOnFilter")?.value ?? '';


    // Determină sortarea: se folosește sort primit ca parametru sau selectul curent
    const sortOrder = (sort === null || sort === "none") ? filter : sort;

    // Folosim searchString dacă este trecut, altfel inputul vizibil
    const search = searchStringCerere || searchInput;

    loadFilteredCereri(pageNumber,sortOrder,search, filter, nrCrt, description, creat_de, sters_de, data_creare, data_stergere);

                            } catch (err) {
        console.error("Eroare la încărcarea filtrelor:", err);
    alert("A apărut o eroare la încărcarea datelor. Verifică consola pentru detalii.");
                            }
                        }

    function loadFilteredCereri(pageNumber = 1, sort = null, searchStringCerere = '', filter='', nrCrt = null, description = '', creat_de='', sters_de='', data_creare='', data_stergere='') {
                                  //alert(sort);
                                  // Construim URL-ul cu parametrii necesari

                                                          const url = `@Url.Action("FilterCereriPartial", "Cereri")?pageNumber=${pageNumber}` +
    `&sortOrder=${encodeURIComponent(sort)}` +
    `&searchString=${encodeURIComponent(searchStringCerere)}` +
    `&filter=${encodeURIComponent(filter)}` +
    `&nrCrt=${encodeURIComponent(nrCrt)}` +
    `&description=${encodeURIComponent(description)}` +
    `&creat_de=${encodeURIComponent(creat_de)}` +
    `&sters_de=${encodeURIComponent(sters_de)}`+
    `&data_creare=${encodeURIComponent(data_creare)}`
    + `&data_stergere=${encodeURIComponent(data_stergere)}`;

    // Fetch AJAX pentru încărcarea partial view
    fetch(url)
                                      .then(res => res.text())
                                      .then(html => {
        document.getElementById("ContainerTabel").innerHTML = html;
    initializeAutocomplete();

                                      })
                                      .catch(err => console.error(err));
                                }


                        document.getElementById("filterForm")?.addEventListener("submit", e => {
        e.preventDefault();
    loadFilteredCereri();
                        });



    // Resetăm toate filtrele și reîncărcăm lista
    function clearFilters() {
        console.log("clearFilters called");

    document.getElementById("filterSelect").value = '';
    document.getElementById("searchField").value = '';
    document.getElementById("nrCrtFilter").value = '';
    document.getElementById("descriptionFilter").value = '';
    document.getElementById("createdByFilter").value = '';
    document.getElementById("deletedByFilter").value = '';
    document.getElementById("createdOnFilter").value = '';
    document.getElementById("deletedOnFilter").value = '';

    // Reîncărcăm lista fără filtre
    loadFilteredInputs(1, null, '', '', null, '', '', '', '');
             }




    function openDeleteModal(id) {
        fetch(`@Url.Action("GetDeleteModal", "Cereri")?id=${id}`)
            .then(res => res.text())
            .then(html => {
                document.getElementById("deleteModalContent").innerHTML = html;
                new bootstrap.Modal(document.getElementById('deleteModal')).show();
            })
            .catch(err => console.error(err));
                        }

    function openDetaliiModal(id) {
        fetch(`@Url.Action("GetDetaliiModal", "Cereri")?id=${id}`)
            .then(res => res.text())
            .then(html => {
                document.getElementById("detaliiModalContent").innerHTML = html;
                new bootstrap.Modal(document.getElementById('detaliiModal')).show();
            })
            .catch(err => console.error(err));
                        }

    function openEditModal(id) {
        fetch(`@Url.Action("GetEditModal", "Cereri")?id=${id}`)
            .then(res => res.text())
            .then(html => {
                document.getElementById("editModalContent").innerHTML = html;
                new bootstrap.Modal(document.getElementById('editModal')).show();
            })
            .catch(err => console.error(err));
                        }



    function deleteCerere(id) {
                            if (!confirm("Sigur dorești să dezactivezi această cerere?")) return;
    fetch(`/Cereri/DeleteConfirmed?id=${id}`, {
        method: "POST",
    headers: {"Content-Type": "application/json" },
    body: JSON.stringify({id})
                            })
                            .then(res => res.json())
                            .then(response => {
                                if (response.success) {
        bootstrap.Modal.getInstance(document.getElementById('deleteModal'))?.hide();
    loadFilteredCereri();
                                } else console.warn(response.message);
                            })
                            .catch(err => {console.error(err); alert("Eroare la dezactivare!"); });
                        }

    function exportCereriExcel() {
                            const filter = document.getElementById("filterSelect")?.value || 'toate';
    window.location.href = `@Url.Action("FilterCereri", "Cereri")?filter=${encodeURIComponent(filter)}&exportExcel=true`;
                        }
