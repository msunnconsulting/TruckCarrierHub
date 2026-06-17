(function (global) {

    var reviewVm = global.reviewVm  = {};

    // Public methods exposed to global scope
    reviewVm.onAddReviewClick = onAddReviewClick;         // Open Add Review modal
    reviewVm.closeReviewIframe = closeReviewIframe;       // Close modal overlay
    reviewVm.openReviewOverlay = openReviewOverlay;       // Open reviews list overlay
    reviewVm.onSignUpBtnClick = onSignUpBtnClick;         // Trigger signup/login popup
    reviewVm.onSubmitPostReview = onSubmitPostReview;     // Submit Add/Edit Review via AJAX
    reviewVm.selectSortOption = selectSortOption;         // Sort reviews
    reviewVm.onReplyReview = onReplyReview;               // Open Add Reply modal
    reviewVm.onEditReview = onEditReview;                 // Open Edit Review modal
    reviewVm.onClickMore = onClickMore;                   // Expand truncated review text
    reviewVm.onSubmitPostResponse = onSubmitPostResponse; // Submit Add/Edit Response via AJAX
    reviewVm.onEditReviewReply = onEditReviewReply;       // Open Edit Reply modal

    /**
     * Opens the Add Review modal for a company.
     * Loads the AddReview partial view via AJAX and binds form events.
     */
    function onAddReviewClick(companyId,isFirstReview) {
        window.companyIdForReview = companyId;
        window.isAddReview = true;

        $.ajax({
            url: '/on-add-review',
            data: { companyId: companyId },
            type: 'POST',
            success: function (response) {

                const modalBox = document.querySelector("#reviewOverlay .modal-box");
                const modalContent = document.getElementById("modalContent");

                modalBox.classList.remove("full-height", "small-height", "review-list");
                modalBox.classList.add("small-height");

                modalContent.innerHTML = response;

                // Wire up Cancel + Post buttons after content is loaded
                $("#btnCancel").on("click", function () {
                    if (isFirstReview) {
                        reviewVm.closeReviewIframe();
                    } else {
                        reviewVm.openReviewOverlay(companyId);
                    }
                });

                $("#reviewAddEditForm").on("submit", function (e) {
                    e.preventDefault();
                    reviewVm.onSubmitPostReview();
                });

                $('#reviewOverlay').fadeIn(300);

                bindReviewFormEvents();
            }
        });
    }

    /**
     * Closes the review modal overlay.
     */
    function closeReviewIframe() {
        $('#reviewOverlay').fadeOut(300);
        $('body').removeClass('modal-open');
        window.location.reload();
    }

    /**
     * Opens the review overlay showing all reviews for a company.
     * Loads reviews list via AJAX.
     */
    function openReviewOverlay(companyId) {
        $.ajax({
            url: '/get-reviews',
            data: { companyUSDOT: companyId },
            type: 'GET',
            success: function (response) {
                const modalBox = document.querySelector("#reviewOverlay .modal-box");
                const modalContent = document.getElementById("modalContent");

                // reset classes
                modalBox.classList.remove("full-height", "small-height", "review-list");

                // 👇 for reviews list → force 80%
                modalBox.classList.add("review-list");

                // lock background scroll
                $('body').addClass('modal-open');

                modalContent.innerHTML = response;
                $('#reviewOverlay').fadeIn(300);
            }
        });
    }

    /**
     * Re-fetches reviews sorted by the selected option.
     */
    function selectSortOption(companyId, sortOption) {
        $.ajax({
            url: '/get-reviews',
            data: { companyUSDOT: companyId, sortDir: sortOption },
            type: 'GET',
            success: function (response) {
                $('#modalContent').html(response);
            }
        });
    }

    /**
     * Opens the login/signup popup window for new users.
     */
    function onSignUpBtnClick() {
        reviewVm.closeReviewIframe();
        const width = 800;
        const height = 850;
        const left = window.screenX + (window.outerWidth - width) / 2;
        const top = window.screenY + (window.outerHeight - height) / 2;

        const loginUrl = "/Login";

        const popup = window.open(
            loginUrl,
            "LoginPopup",
            `width=${width},height=${height},top=${top},left=${left},resizable=no,scrollbars=no,toolbar=no,location=no,status=no,menubar=no`
        );

        if (popup) popup.focus();
        else alert("Popup blocked. Please allow popups for this site.");
    }

    /**
     * Submits the Add/Edit Review form via AJAX.
     */
    function onSubmitPostReview() {
        var form = $("#reviewAddEditForm");

        var formData = {
            CompanyUSDOT:  form.find("input[name='CompanyUSDOT']").val(),
            ReviewerUSDOT: form.find("input[name='ReviewerUSDOT']").val(),
            Rating: form.find("input[name='Rating']:checked").val(),
            Comment: form.find("#Comment").val(),
            ReviewId: form.find("input[name='ReviewId']").val()
        };

        $.ajax({
            url: '/on-submit-review',
            type: 'POST',
            data: formData,
            success: function (response) {
                reviewVm.openReviewOverlay(formData.CompanyUSDOT);
            },
            error: function () {
                $('.p-35-px').addClass('alt-padding');
                $('.margin-top-35').addClass('margin-top-15');
                $('.margin-top-35').removeClass("margin-top-35");
            }
        });
    }

    /**
     * Validates review form:
     * - New review → requires at least 1 star.
     * - Edit review → requires changes in rating or comment.
     */
    function bindReviewFormEvents() {
        var isEdit = parseInt($("#hdnReviewId").val()) > 0;
        var originalRating = parseInt($("#hdnOriginalRating").val()) || 0;
        var originalComment = $("#hdnOriginalComment").val() || "";
        var $btnPost = $("#btnPost");
        var $comment = $("#Comment");
        var $ratingInputs = $("input[name='Rating']");

        // disable by default
        $btnPost.prop("disabled", true);

        if (!isEdit) {
            // Add Review → must select at least one star
            $ratingInputs.on("change", function () {
                $btnPost.prop("disabled", $ratingInputs.filter(":checked").length === 0);
            });
        } else {
            // Edit Review → must change rating or comment
            $ratingInputs.add($comment).on("change keyup", function () {
                var ratingChanged = ($ratingInputs.filter(":checked").val() != originalRating);
                var commentChanged = ($comment.val().trim() !== originalComment.trim());
                $btnPost.prop("disabled", !(ratingChanged || commentChanged));
            });
        }
    }

    /**
     * Validates response form:
     */
    function bindResponseFormEvents() {
        var isEdit = parseInt($("#hdnResponseId").val()) > 0;
        var originalResponse = $("#Response").val() || "";
        var $btnPost = $("#btnPost");
        var $response = $("#Response");

        // Disable the Post/Update button initially
        $btnPost.prop("disabled", true);

        if (!isEdit) {
            // Add Response → must type something
            $response.on("keyup change", function () {
                var hasText = $response.val().trim().length > 10;
                $btnPost.prop("disabled", !hasText);
            });
        } else {
            // Edit Response → must change text
            $response.on("keyup change", function () {
                var responseChanged = ($response.val().trim() !== originalResponse.trim());
                var hasEnoughText = $response.val().trim().length >= 10;
                $btnPost.prop("disabled", !(responseChanged && hasEnoughText));
            });
        }
    }


    /**
     * Opens Edit Review modal.
     * Loads review data via AJAX and binds form events.
     */
    function onEditReview(reviewId, companyId) {

       $.ajax({
           url: '/on-edit-review',
           data: { reviewId: reviewId },
           type: 'POST',
           success: function (response) {
               const modalBox = document.querySelector("#reviewOverlay .modal-box");
               const modalContent = document.getElementById("modalContent");

               modalBox.classList.remove("full-height", "small-height", "review-list");
               modalBox.classList.add("small-height");

               modalContent.innerHTML = response;

               // Wire up Cancel + Post buttons after content is loaded
               $("#btnCancel").on("click", function () {
                   reviewVm.openReviewOverlay(companyId);
               });

               $("#reviewAddEditForm").on("submit", function (e) {
                   e.preventDefault();
                   reviewVm.onSubmitPostReview();
               });

               $('#reviewOverlay').fadeIn(300);

               bindReviewFormEvents();
           }
       });
    }

    /**
     * Opens Add Reply modal for a review.
     */
    function onReplyReview(reviewId, companyId) {
        $.ajax({
            url: '/on-add-review-reply',
            data: { reviewId: reviewId, companyId: companyId },
            type: 'POST',
            success: function (response) {

                const modalBox = document.querySelector("#reviewOverlay .modal-box");
                const modalContent = document.getElementById("modalContent");

                modalBox.classList.remove("full-height", "small-height", "review-list");
                modalBox.classList.add("small-height");

                modalContent.innerHTML = response;

                // Wire up Cancel + Post buttons after content is loaded
                $("#btnCancel").on("click", function () {
                    reviewVm.openReviewOverlay(companyId);
                });

                $("#reviewAddEditForm").on("submit", function (e) {
                    e.preventDefault();
                    reviewVm.onSubmitPostResponse();
                });

                $('#reviewOverlay').fadeIn(300);

                bindResponseFormEvents();
            }
        });
    }

    /**
     * Submits the Add/Edit Response (reply to review) form via AJAX.
     */
    function onSubmitPostResponse() {
        var form = $("#responseAddEditForm");

        var formData = {
            Id: form.find("input[name='Id']").val(),
            ReviewId: form.find("input[name='ReviewId']").val(),
            CompanyUSDOT: form.find("input[name='CompanyUSDOT']").val(),
            Response: form.find("#Response").val()
        };

        $.ajax({
            url: '/add-edit-review-response',
            type: 'POST',
            data: formData,
            success: function (response) {
                reviewVm.openReviewOverlay(formData.CompanyUSDOT);

                // Clean up the browser URL by removing query parameters (reviewId & open),
                // so the page won’t keep reopening the review modal on reload/refresh.
                // pushState updates the URL without reloading the page.
                const cleanUrl = window.location.protocol + "//" + window.location.host + window.location.pathname;
                window.history.pushState({ path: cleanUrl }, "", cleanUrl);
            }
        });
    }

    /**
     * Expands truncated review text (when "More" link clicked).
     */
    function onClickMore() {
        // Event delegation for dynamically loaded content
        $(document).on("click", ".toggle-text", function (e) {
            e.preventDefault();

            var $this = $(this);
            var $container = $this.closest(".review-text");

            var $preview = $container.find(".preview-text");
            var $full = $container.find(".full-text");

            // Show full text and hide preview, remove the "More" link
            $preview.hide();
            $full.show();
            $this.remove();
        });
    }

    /**
     * Opens Edit Reply modal for a review response.
     */
    function onEditReviewReply(responsId, companyId) {
        $.ajax({
            url: '/on-edit-review-response',
            data: { responseId: responsId },
            type: 'POST',
            success: function (response) {

                const modalBox = document.querySelector("#reviewOverlay .modal-box");
                const modalContent = document.getElementById("modalContent");

                modalBox.classList.remove("full-height", "small-height", "review-list");
                modalBox.classList.add("small-height");

                modalContent.innerHTML = response;

                // Wire up Cancel + Post buttons after content is loaded
                $("#btnCancel").on("click", function () {
                    reviewVm.openReviewOverlay(companyId);
                });

                $("#responseAddEditForm").on("submit", function (e) {
                    e.preventDefault();
                    reviewVm.onSubmitPostResponse();
                });

                $('#reviewOverlay').fadeIn(300);

                bindResponseFormEvents();
            }
        });
    }

    // Auto-bind click handler for "More" links on DOM ready
    $(document).ready(function () {
        reviewVm.onClickMore();
    });

})(this);
