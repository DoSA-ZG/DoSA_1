$(document).on('click', '.delete-row', function (event) {
    event.preventDefault();
    const tr = $(this).parents("tr");
    tr.remove();
  });

  $(function () {
    $("#operation-add").click(function (event) {
      event.preventDefault();
      addOperation();
    });
  });
  
  function addOperation() {
    const operationTypeName = parseInt($("#operation-typename").val());
    const operationCost = parseInt($("#operation-cost").val());
    const operationStatus = $("#operation-status").is(":checked");
    const operationDate = new Date($("#operation-date").val());
    const operationAmount = parseInt($("#operation-amount").val());
    const operationPlantId = parseInt($("#operation-plantid").val());
    console.log(operationPlantId);

    if (isNaN(operationTypeName)){
        alert("Please, choose the operation type");
        return;
    }
    else if (isNaN(operationCost)){
        alert("Cost must be set");
        return;
    }
    else if (isNaN(operationStatus)){
        alert("Status must be set");
        return;
    }
    else if (!operationDate){
        alert("Date must be set");
        return; 
        
    }
    else if (isNaN(operationAmount) || operationAmount < 1){
        alert("Amount must be set and must be a positive integer");
        return;
    }
    else if (isNaN(operationPlantId)){
        alert("Please choose the plant from dropdown");
        return;
    }
    const operationId = new Date().getTime();
    if (operationStatus){
      var opStatus = "true"
    }
    else{
      var opStatus = "false"
    }

    let template = $('#template').html();
    template = template.replace(/--name--/g, operationTypeName.toString())
    .replace(/--id--/g, operationId)
    .replace(/--cost--/g, operationCost.toString())
    .replace(/--status--/g, opStatus)
    .replace(/--date--/g, operationDate.toISOString().split('T')[0])
    .replace(/--amount--/g, operationAmount)
    .replace(/--plantid--/, operationPlantId)

    $(template).find('tr').insertBefore($("#table-order").find('tr').last());
    $("#table-order").find(`select[name="Orders[${id}].OrderId"]`).find(`option[value='${operationPlantId}']`).prop('selected', true);

    $("#operation-id").val('');
    $("#operation-typename").val('');
    $("#operation-cost").val('');
    $("#operation-status").val('');
    $("#operation-date").val('');
    $("#operation-amount").val('');
    $("#operation-plantid").val('');
    
  }