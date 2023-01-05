
export const environment = {
  production: true,
  settings: {
    appname: 'jArchive',
    oidc: {
      client_id: 'client_id',
      authority: '',
      redirect_uri: 'https://app_url/oidc',
      silent_redirect_uri: 'https://app_url/assets/oidc-silent.html',
      response_type: 'code',
      scope: 'openid profile jarchive-api',
      monitorSession: false,
      loadUserInfo: false,
      automaticSilentRenew: true,
      useLocalStorage: true,
      debug: false
    }
  }
};
