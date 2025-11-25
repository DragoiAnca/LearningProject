document.addEventListener("DOMContentLoaded", function () {
    const errorDiv = document.getElementById("errorMessages");
    const fileInput = document.getElementById("customFile");
    const noFilesMessage = document.getElementById("noFilesMessage");
    const draftInput = document.querySelector("input[name='DraftId']");
    let draftId = draftInput ? draftInput.value : 0;
    const inputs = document.querySelectorAll("input[type='text'], input[type='number']");

    if (!fileInput) return;

    async function saveDraft(data = {}) {
        const formData = new FormData();
        formData.append("DraftId", draftId);

        for (const key in data) {
            if (key === "UploadedFiles") {
                for (let f of data[key]) formData.append("UploadedFiles", f);
            } else {
                formData.append(key, data[key]);
            }
        }

        try {
            const resp = await fetch("/Cereri/SaveDraft", {
                method: "PUT",
                body: formData
            });
            const result = await resp.json();

            if (result.DraftId) {
                draftId = result.DraftId;
                if (draftInput) draftInput.value = draftId;
            }

            // Actualizare lista de fișiere
            const tableBody = document.querySelector("#existingFilesTable tbody");
            if (result.files && result.files.length > 0) {

                // Ascundem mesajul "Niciun fișier selectat"
                if (noFilesMessage) noFilesMessage.style.display = "none";

                result.files.forEach(f => {
                    if (f.alreadyExists) {
                        console.warn(`Fișierul ${f.fileName} exista deja și nu a fost adăugat`);

                        if (errorDiv) {
                            const p = document.createElement("p");
                            p.textContent = `Fișierul ${f.fileName} exista deja și nu a fost adăugat`;
                            errorDiv.appendChild(p);
                        }

                    } else {
                        errorDiv.innerHTML = "";

                        const tr = document.createElement("tr");

                        const tdName = document.createElement("td");
                        tdName.textContent = f.fileName;

                        const tdAction = document.createElement("td");

                        const a = document.createElement("a");
                        a.href = f.filePath;
                        a.target = "_blank";

                        a.innerHTML = '<i class="bi bi-trash3"></i>';

                        a.classList.add("btn", "btn-danger", "btn-xl", "btn-circle");

                        a.setAttribute("title", "Șterge fișierul curent");

                        a.setAttribute("data-toggle", "tooltip");
                        a.setAttribute("data-placement", "bottom");

                        tdAction.appendChild(a);

                        tr.appendChild(tdName);
                        tr.appendChild(tdAction);

                        tableBody.appendChild(tr);
                        console.log(`Adaug fisierul ${f.fileName}`);
                    }
                });

            } else {
                // Dacă nu există fișiere
                if (noFilesMessage) {
                    noFilesMessage.style.display = "inline";
                    noFilesMessage.textContent = "Niciun fișier selectat";
                }
            }

        } catch (err) {
            console.error("Eroare la salvarea draft-ului:", err);
        }
    }

    // Autosave pentru inputuri
    inputs.forEach(input => {
        input.addEventListener("focusout", () => saveDraft({ [input.name]: input.value }));
    });

    // Resetăm input-ul ca să permită selectarea aceluiași fișier, altfel nu functioneaza
    fileInput.addEventListener("click", function () {
        this.value = "";
    });




    // Upload live fișiere
    fileInput.addEventListener("change", function () {
        const files = Array.from(this.files);
        console.log("FISIERE SELECTATE:", files.map(f => f.name));

        if (files.length > 0)
            saveDraft({ UploadedFiles: files });
    });
});