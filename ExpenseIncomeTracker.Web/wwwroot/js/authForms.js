window.authForms = {
  postJson: function (url, payload) {
    return fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(payload),
      credentials: "include"
    }).then(function (response) {
      return response.json();
    });
  }
};
