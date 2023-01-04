export const environment = {
  production: false,
  settings: {
    appname: 'jArchive',
    apphost: 'http://localhost:5272',
    oidc: {
      client_id: 'dev-code',
      authority: 'http://localhost:5000',
      redirect_uri: 'http://localhost:4200/oidc',
      silent_redirect_uri: 'http://localhost:4200/assets/oidc-silent.html',
      response_type: 'code',
      scope: 'openid profile dev-api',
      monitorSession: false,
      loadUserInfo: true,
      automaticSilentRenew: true,
      useLocalStorage: true,
      debug: false
    }
  }
};
