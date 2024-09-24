$(document).on('click', '.delete-row', function (event) {
    event.preventDefault();
    const tr = $(this).parents("tr");
    tr.remove();
  });

  $(function () {
    $("#order-add").click(function (event) {
      event.preventDefault();
      addOrder();
    });
  });
  
  function addOrder() {
    const orderDate = $("#order-date").val();
    if (orderDate !== '') { 

      let template = $('#template').html();

      const id = new Date().getTime();
  
      template = template.replace(/--date--/g, orderDate)
        .replace(/--id--/g, id)

      $(template).find('tr').insertBefore($("#table-order").find('tr').last());
  
      $("#order-id").val('');
      $("#order-date").val('');
    } else {
      alert("Please, choose the correct date");
    }
  }