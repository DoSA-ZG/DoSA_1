$(document).on('click', '.delete-row', function (event) {
    event.preventDefault();
    const tr = $(this).parents("tr");
    tr.remove();
  });

  $(function () {
    $("#plant-add").click(function (event) {
      event.preventDefault();
      addPlant();
    });
  });
  
  function addPlant() {
    const speciesId = $("#plant-species").val();
    if (speciesId !== '') { 
      let quantity = parseInt($("#plant-quantity").val());

      if (isNaN(quantity) || quantity < 1) {
        alert("Quantity must be set and must be a positive integer");
        return;
      }

      let purpose = parseInt($("#plant-purpose").val());
      if (isNaN(purpose)) {
        alert("Purpose must be chosen");
        return;
      }

      console.log(purpose);
      let nv = $("#plant-nv").val();

      console.log(nv);
      console.log(quantity)
  
      let template = $('#template').html();
      const name = $("#plant-name").val();

      const id = new Date().getTime();
  
      template = template.replace(/--speciesId--/g, speciesId)
        .replace(/--id--/g, id)
        .replace(/--quantity--/g, quantity)
        .replace(/--name--/g, name)
        .replace(/--nv--/, nv)

      $(template).find('tr').insertBefore($("#table-plant").find('tr').last());
      $("#table-plant").find(`select[name="Plants[${id}].PurposeId"]`).find(`option[value='${purpose}']`).prop('selected', true);
  
      $("#plant-id").val('');
      $("#plant-quantity").val('');
      $("#plant-purpose").val('');
      $("#plant-nv").val('');
      $("#plant-name").val('');
    } else {
      alert("Please, choose the species using autocomplete");
    }
  }