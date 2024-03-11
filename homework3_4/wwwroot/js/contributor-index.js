
$(function ()  {
    $("#new-contributor").on('click', function () {
        new bootstrap.Modal($('.new-contrib')[0]).show();
    });
    $(".edit-contrib").on('click', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');
        const cell = $(this).data('cell');
        const alwaysInclude = $(this).data('always-include');
        const dateCreated = $(this).data('date');

        const form = $(".new-contrib form");
        form.find("#edit-id").remove();
        const hidden = $(`<input type='hidden' id='edit-id' name='id' value='${id}' />`);
        form.append(hidden);

        $("#initialDepositDiv").hide();

        $("#contributor_name").val(name);
        $("#contributor_cell").val(cell);
        $("#contributor_always_include").prop('checked', alwaysInclude === "True");
        $("#contributor_date").val(dateCreated.ToShortDateString());
        new bootstrap.Modal($('.new-contrib')[0]).show();
        form.attr('action', '/home/update');
    });

    $(".deposit-button").on('click', function () {
        const contribId = $(this).data('contribid');
        $('[name="contributorId"]').val(contribId);

        const tr = $(this).closest('tr');
        const name = tr.find('td:eq(1)').text();
        $("#deposit-name").text(name);

        new bootstrap.Modal($('.deposit')[0]).show();
    });

    $("#search").on('keyup', function () {
        const text = $(this).val();
        $("table tr:gt(0)").each(function () {
            const tr = $(this);
            const name = tr.find('td:eq(1)').text();
            if (name.toLowerCase().indexOf(text.toLowerCase()) !== -1) {
                tr.show();
            } else {
                tr.hide();
            }
        });
    });

    $("#clear").on('click', function () {
        $("#search").val('');
        $("tr").show();
    });
});
