function majPanier(url, id) {
    $.post(url, { "id": id }, function () {
        window.location.href = "/Panier";
    });
}

$(document).on("click", ".PlusProducts", function (event) {
    event.preventDefault();
    majPanier("/Panier/PlusProduct", $(this).attr("data-id"));
});

$(document).on("click", ".MinProducts", function (event) {
    event.preventDefault();
    majPanier("/Panier/MinusProduct", $(this).attr("data-id"));
});

$(document).on("click", ".RemoveLink", function (event) {
    event.preventDefault();
    majPanier("/Panier/RemoveProduct", $(this).attr("data-id"));
});