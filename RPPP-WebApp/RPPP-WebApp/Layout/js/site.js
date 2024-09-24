$(document).ready(function() {
    doPaging(20); //instead of 20 we need to get the number of pages from backend
    window.entity = $("table").attr("data-entity");

    handleButtons();
    analyseQuery();

    $("#paging span:not([id^='current']):not([id^='minGap']):not([id^='maxGap'])").on("click", (event) => {
        event.preventDefault();
        const pageNum = $(event.target).closest("span").attr("data-page");
        
        let params = new URLSearchParams(window.location.search);
        params.delete("page");
        params.append("page", pageNum);

        moveToPage(params.toString());
    });

    $("#menu .dropdown").on("click", (event) => {
        const currentDropdown = event.target.closest(".dropdown");
        $.makeArray($("#menu .dropdown")).map((oneElem) => {
            const arrow = oneElem.querySelector('i');

            if (!currentDropdown.isSameNode(oneElem)) {
                oneElem.querySelector(".dropdown-content").classList.add('hidden');
                arrow.classList.remove('fa-angle-down');
                arrow.classList.add('fa-angle-right');
            } else {
                const currentContent = currentDropdown.querySelector(".dropdown-content");
                if (currentContent.classList.contains('hidden')) {
                    currentContent.classList.remove('hidden');
                    arrow.classList.remove('fa-angle-right');
                    arrow.classList.add('fa-angle-down');
                } else {
                    currentContent.classList.add('hidden');
                    arrow.classList.remove('fa-angle-down');
                    arrow.classList.add('fa-angle-right');
                }
            }
        });
    })

    $("#menu .dropdown a").on("click", (event) => {
        event.preventDefault();
        moveToPage("", $(event.target).attr("href"));
    })

    $("form#modal-form").on("submit", (event) => {
        event.preventDefault();

        // $(event).validate({
        //     rules: {
        //         country: {
        //             required: true
        //         },
        //         email: {
        //             required: true
        //         },
        //         phone: {
        //             required: true,
        //             pattern: /[0-9]{3}-[0-9]{3}-[0-9]{4}/
        //         }
        //     },
        //     messages: {
        //         phone: {
        //             required: "Please enter your phone number",
        //             pattern: "Please enter a valid phone number in the format 123-456-7890"
        //         }
        //     }
        // });

        // Create a FormData object directly from the event
        var formData = new FormData(event.target);
        
        result = checkFields(formData);

        if (result !== true) {
            //we should add the feedback for the fields here
        }

        // $("#changeModal").modal('hide');
        let url = '/' + window.entity;
        type = "POST"

        if (window.action == "edit") {
            type = "PUT";
            if (window.id) url += `/${window.id}`;
        }

        doRequest(type, url, false, formData);
    });
});

function checkFields(data) {
    return true;
}

function analyseQuery() {
    const params = new URLSearchParams(window.location.search);

    params.forEach((value, key) => {
        $(`th[data-column-name=${key}]`).find(`.${value}`).addClass("selected");
    })
}

function handleButtons() {
    handleModalButtons();

    $("button#reset-filters").on("click", (event) => {
        let query = new URLSearchParams(window.location.search);

        $(".sorting-icons i").each((key, el) => {
            $(el).removeClass('selected');
            query.delete($(el).closest("th").attr("data-column-name"));
        });

        moveToPage(query.toString());
    })

    $(".sorting-icons i").on("click", (event) => {
        const elem = $(event.target);
        let urlParams = new URLSearchParams(window.location.search);
        const columnName = elem.closest("th").attr("data-column-name");

        urlParams.delete(columnName)

        if (elem.hasClass('selected')) {
            elem.removeClass('selected');
        } else {
            elem.closest(".sorting-icons").find('i').each((key, el) => {$(el).removeClass('selected');})
            elem.addClass('selected');

            urlParams.append(columnName, elem.hasClass('asc') ? 'asc' : 'desc');
        }

        moveToPage(urlParams.toString());
    });
}

function moveToPage(query = "", entity = "") {
    let base = window.location.origin;
    if (!entity) {
        base += window.location.pathname
    }
    const url = `${base}?${query}`;

    window.location.replace(url);
}

function handleModalButtons() {
    $("#changeModal button[data-dismiss]").on("click", (event) => {
        $(event.target).closest("#changeModal").find("#modal-form").trigger("reset");
    });

    $("button[data-target='#changeModal']").on("click", (event) => {
        const item = $(event.target).closest("button");
        const action = item.attr("data-action");

        $(".modal-title #title-action").text(action.toLowerCase());

        if (action === "edit") {
            window.action = "edit";
            const row = item.closest("tr");
            window.id = row.attr("data-element-id");

            //should be a request to the db by id of the element
            row.find("td:not(:last-child)").each((index, oneColumn) => {
                const fieldName = oneColumn.getAttribute('data-name');
                let field = $(`.modal-field #${fieldName}`);
                let value = oneColumn.innerText;

                if (field.attr("type") === "date") {
                    value = value && (new Date(value)).toISOString().split('T')[0]
                }

                field?.val(value);
            })
        } else {
            window.action = "change";
            const modalId = $(event.target).closest("button").attr("data-target");
            $(`${modalId}`).find("form").trigger("reset")
        }
    })

    $("button.delete").on("click", (event) => {
        window.action = "delete";
        window.id = $(event.target).closest("tr").attr("data-element-id");
    });

    $("#deleteModal button[type=submit]").on("click", (event) => {
        let url = '/' + window.entity;
        if (window.id) url += '/' + window.id;
        window.id = null;
        doRequest("DELETE", url || "");
    });
}

function doPaging(maxPage) {
    const urlPage = parseInt((new URLSearchParams(window.location.search)).get('page')) || 1;
    let newPage;

    if (urlPage && urlPage > maxPage) newPage = maxPage;
    else if (!urlPage) newPage = 1;
    else newPage = urlPage;

    const nextPage = Math.min(maxPage, newPage + 1);
    const prevPage = Math.max(1, newPage - 1);
    
    $("#current").text(newPage);
    $("#prev").attr('data-page', prevPage);
    $("#next").attr('data-page', nextPage);

    let next = $("#current").next("span");
    if (nextPage == maxPage) next.remove();
    else {
        next.attr('data-page', nextPage);
        next.text(nextPage);
    }

    if (newPage == maxPage) {
        next.remove();
        $("#maxPage").remove();
        $("#maxGap").remove()
        $("#next").addClass("disabled");
    } else {
        $("#maxPage").attr('data-page', maxPage);
        $("#maxPage").text(maxPage);
    }
    

    let prev = $("#current").prev("span");
    if (prevPage == 1) prev.remove();
    else {
        prev.attr('data-page', prevPage);
        prev.text(prevPage);
    }

    if (newPage == 1) {
        $("#minPage").remove();
        $('#prev').addClass("disabled");
    } else {
        $("#minPage").attr('data-page', 1);
        $("#minPage").text(1);
    }

    if (newPage - 1 <= 2) $("#minGap").remove();
    if (maxPage - newPage <= 2) $("#maxGap").remove();

    $("#paging").removeClass("hidden");
}

function doRequest(type, url, isReturn = false, data = undefined) {
    $.ajax({
        url: url, //some url we need
        type: type,
        data: data,
        processData: false,
        contentType: false,
        success: function(response) {
            if (type.toLowerCase() != "get") {
                showToast(response.message, 'success');
                window.location.reload();
            }
        },
        error: function(request, msg, error) {
            showToast('Error: ' + error, 'danger');
        }
    });
}

function showToast(message, type) {
    var toastContainer = $('#toast-container');

    let toast = $('<div class="toast" role="alert" aria-live="assertive" aria-atomic="true">\
                    <div class="toast-header">\
                    <strong class="mr-auto">Bootstrap Toast</strong>\
                    <button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">\
                        <span aria-hidden="true">&times;</span>\
                    </button>\
                    </div>\
                    <div class="toast-body"></div>\
                </div>');

    // Customize the content and classes based on the response type
    toast.find('.toast-body').text(message);
    toast.find('.mr-auto').text(window.action);
    toast.removeClass('bg-success bg-danger').addClass('bg-' + type);

    toastContainer.append(toast);
    toast.toast({ delay: 4000 }).toast('show');
  }