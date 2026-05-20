(function () {
  var baseUrl = "https://xobinteam.xobin.com/wc/assessment/";
  var linkPattern = /^https:\/\/xobinteam\.xobin\.com\/wc\/assessment\/([A-Za-z0-9]+)\?inviteToken=([A-Za-z0-9]+)$/;
  var tokenPattern = /^[A-Za-z0-9]+$/;

  var form = document.getElementById("landing-form");
  var error = document.getElementById("form-error");
  var nameInput = document.getElementById("candidate-name");
  var emailInput = document.getElementById("candidate-email");
  var assessmentInput = document.getElementById("assessment-id");
  var tokenInput = document.getElementById("invite-token");
  var linkInput = document.getElementById("assessment-link");

  function setError(message) {
    error.textContent = message || "";
  }

  function normalize(value) {
    return (value || "").trim();
  }

  function buildAssessmentUrl() {
    var directLink = normalize(linkInput.value);
    var assessmentId = normalize(assessmentInput.value);
    var inviteToken = normalize(tokenInput.value);

    if (directLink) {
      var directMatch = directLink.match(linkPattern);

      if (!directMatch) {
        throw new Error("Enter a valid Xobin assessment link in the required format.");
      }

      assessmentInput.value = directMatch[1];
      tokenInput.value = directMatch[2];

      return directLink;
    }

    if (!assessmentId || !inviteToken) {
      throw new Error("Enter an assessment ID and invite token, or paste the complete assessment link.");
    }

    if (!tokenPattern.test(assessmentId) || !tokenPattern.test(inviteToken)) {
      throw new Error("Assessment ID and invite token can contain only letters and numbers.");
    }

    return baseUrl + encodeURIComponent(assessmentId) + "?inviteToken=" + encodeURIComponent(inviteToken);
  }

  function rememberCandidate(assessmentUrl) {
    try {
      localStorage.setItem("xolockCandidate", JSON.stringify({
        name: normalize(nameInput.value),
        email: normalize(emailInput.value),
        assessmentId: normalize(assessmentInput.value),
        inviteToken: normalize(tokenInput.value),
        assessmentUrl: assessmentUrl,
        submittedAt: new Date().toISOString()
      }));
    } catch (error) {
      return;
    }
  }

  linkInput.addEventListener("input", function () {
    var directMatch = normalize(linkInput.value).match(linkPattern);

    if (directMatch) {
      assessmentInput.value = directMatch[1];
      tokenInput.value = directMatch[2];
      setError("");
    }
  });

  form.addEventListener("submit", function (event) {
    event.preventDefault();
    setError("");

    if (!nameInput.value.trim()) {
      setError("Enter the candidate name.");
      nameInput.focus();
      return;
    }

    if (!emailInput.validity.valid) {
      setError("Enter a valid email address.");
      emailInput.focus();
      return;
    }

    try {
      var assessmentUrl = buildAssessmentUrl();
      rememberCandidate(assessmentUrl);

      if (document.documentElement.requestFullscreen) {
        document.documentElement.requestFullscreen().catch(function () {});
      }

      window.location.assign(assessmentUrl);
    } catch (validationError) {
      setError(validationError.message);
    }
  });
}());
