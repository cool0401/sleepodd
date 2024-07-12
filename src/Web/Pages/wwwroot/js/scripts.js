redirectToCheckout = function (sessionId) {
    var stripe = Stripe('pk_test_51P93oC2NqsvUilZCygeY4UPaKy0PLLvgT2BxNzyBvyZjWfuYhxeUGMgOUZivHa0nyXc245Un6JeFsGGtL2ABns7M00oB7dY8Q8');
    stripe.redirectToCheckout({
        sessionId: sessionId
    }).then(function (result) {
        if (result.error) {
            var displayError = document.getElementById("stripe-error");
            displayError.textContent = result.error.message;
        }
    });
};