function validateOrder() {
    var context = getContext();
    var request = context.getRequest();
    var item = request.getBody();
    if (item["OrderCustomer"] != undefined && item["OrderCustomer"] != null) {
        request.setBody(item); 
    }
    else {
        throw new Error('OrderCustomer must be specified');
    }
}