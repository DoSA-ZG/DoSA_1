$(document).ready(function() {
    handleButtons();

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
});

function handleButtons() {
    $('.delete').on('click', (event) => {
        event.preventDefault();

        var form = $(event.target).closest('form');
        var deleteModal = $('#deleteModal');

        deleteModal.modal('show');

        $('#deleteModal button[type="submit"]').on('click', (event) => {
            form.submit();
        });

        deleteModal.on('hidden.bs.modal', function () {
            $('#deleteModal button[type="submit"]').off('click');
        });
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

function doRequest(type, entity, id = null, action = null, isReturn = false, data = undefined) {
    let url = `${window.location.origin}/${entity}/${(action || type).toLowerCase()}` + (id !== null ? `/${id}` : '');
    $.ajax({
        url: url,
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
            let errorText = error;
            if (!errorText) {
                errorText = request.responseText?.substring(0, 300);
                errorText += '...';
            }
            showToast(errorText, 'danger');
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