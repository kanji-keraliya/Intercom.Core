﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intercom.Clients;
using Intercom.Core;
using Intercom.Data;
using Intercom.Exceptions;
using Intercom.Factories;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;

namespace Intercom.Clients
{
    public class UsersClient : Client
    {
        // TODO: Implement paging
        private static class UserSortBy
        {
            public const String created_at = "created_at";
            public const String updated_at = "updated_at";
            public const String signed_up_at = "signed_up_at";
        }

        private const String USERS_RESOURCE = "users";
        private const String PERMANENT_DELETE_RESOURCE = "user_delete_requests";

        public UsersClient(RestClientFactory restClientFactory)
            : base(USERS_RESOURCE, restClientFactory)
        {
        }

        [Obsolete("This constructor is deprecated as of 3.0.0 and will soon be removed, please use UsersClient(RestClientFactory restClientFactory)")]
        public UsersClient(Authentication authentication)
            : base(INTERCOM_API_BASE_URL, USERS_RESOURCE, authentication)
        {
        }

        [Obsolete("This constructor is deprecated as of 3.0.0 and will soon be removed, please use UsersClient(RestClientFactory restClientFactory)")]
        public UsersClient(String intercomApiUrl, Authentication authentication)
            : base(String.IsNullOrEmpty(intercomApiUrl) ? INTERCOM_API_BASE_URL : intercomApiUrl, USERS_RESOURCE, authentication)
        {
        }

        public User Create(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(user.user_id) && string.IsNullOrEmpty(user.email))
            {
                throw new ArgumentException("you need to provide either 'user.user_id', 'user.email' to create a user.");
            }

            ClientResponse<User> result = null;
            result = Post<User>(Transform(user));
            return result.Result;
        }

        public User Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(user.id) && String.IsNullOrEmpty(user.user_id) && string.IsNullOrEmpty(user.email))
            {
                throw new ArgumentException("you need to provide either 'user.id', 'user.user_id', 'user.email' to update a user.");
            }

            ClientResponse<User> result = null;
            result = Post<User>(Transform(user));
            return result.Result;
        }

        private User CreateOrUpdate(User user)
        {
            if (user.custom_attributes != null && user.custom_attributes.Any())
            {
                if (user.custom_attributes.Count > 100)
                    throw new ArgumentException("Maximum of 100 fields.");

                foreach (var attr in user.custom_attributes)
                {
                    if (attr.Key.Contains(".") || attr.Key.Contains("$"))
                        throw new ArgumentException(String.Format("Field names must not contain Periods (.) or Dollar ($) characters. key: {0}", attr.Key));

                    if (attr.Key.Length > 190)
                        throw new ArgumentException(String.Format("Field names must be no longer than 190 characters. key: {0}", attr.Key));

                    if (attr.Value == null)
                        throw new ArgumentException(String.Format("'value' is null. key: {0}", attr.Key));
                }
            }

            ClientResponse<User> result = null;
            result = Post<User>(Transform(user));
            return result.Result;
        }

        public User View(Dictionary<String, String> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (!parameters.Any())
            {
                throw new ArgumentException("'parameters' argument should include user_id parameter.");
            }

            ClientResponse<User> result = null;

            result = Get<User>(parameters: parameters);
            return result.Result;
        }

        public User View(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            ClientResponse<User> result = null;
            result = Get<User>(resource: USERS_RESOURCE + Path.DirectorySeparatorChar + id);
            return result.Result;
        }

        public User View(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Dictionary<String, String> parameters = new Dictionary<string, string>();
            ClientResponse<User> result = null;

            if (!String.IsNullOrEmpty(user.id))
            {
                result = Get<User>(resource: USERS_RESOURCE + Path.DirectorySeparatorChar + user.id);
            }
            else if (!String.IsNullOrEmpty(user.user_id))
            {
                parameters.Add(Constants.USER_ID, user.user_id);
                result = Get<User>(parameters: parameters);
            }
            else if (!String.IsNullOrEmpty(user.email))
            {
                parameters.Add(Constants.EMAIL, user.email);
                var items = Get<Users>(parameters: parameters);
                return items.Result?.users?.FirstOrDefault();
            }
            else
            {
                throw new ArgumentException("you need to provide either 'user.id', 'user.user_id', 'user.email' to view a user.");
            }

            return result.Result;
        }

        public Users List()
        {
            ClientResponse<Users> result = null;
            result = Get<Users>();
            return result.Result;
        }

        public Users List(Dictionary<String, String> parameters)
        {
            ClientResponse<Users> result = null;
            result = Get<Users>(parameters: parameters);
            return result.Result;
        }

        // TODO: Implement paging (by Pages argument)
        private Users Next(Pages pages)
        {
            return null;
        }

        // TODO: Implement paging
        private Users Next(int page = 1, int perPage = 50, OrderBy orderBy = OrderBy.Dsc, String sortBy = UserSortBy.created_at)
        {
            return null;
        }

        public Users Scroll(String scrollParam = null)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            ClientResponse<Users> result = null;

            if (!String.IsNullOrWhiteSpace(scrollParam))
            {
                parameters.Add("scroll_param", scrollParam);
            }

            result = Get<Users>(parameters: parameters, resource: USERS_RESOURCE + Path.DirectorySeparatorChar + "scroll");
            return result.Result;
        }

        public User Archive(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            Dictionary<String, String> parameters = new Dictionary<string, string>();
            ClientResponse<User> result = null;

            if (!String.IsNullOrEmpty(user.id))
            {
                result = Delete<User>(resource: USERS_RESOURCE + Path.DirectorySeparatorChar + user.id);
            }
            else if (!String.IsNullOrEmpty(user.user_id))
            {
                parameters.Add(Constants.USER_ID, user.user_id);
                result = Delete<User>(parameters: parameters);
            }
            else if (!String.IsNullOrEmpty(user.email))
            {
                parameters.Add(Constants.EMAIL, user.email);
                result = Delete<User>(parameters: parameters);
            }
            else
            {
                throw new ArgumentException("you need to provide either 'user.id', 'user.user_id', 'user.email' to view a user.");
            }

            return result.Result;
        }

        [Obsolete("Replaced by Archive(User user). Renamed for consistency with API language.")]
        public User Delete(User user)
        {
            return Archive(user);
        }

        public User Archive(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            ClientResponse<User> result = null;
            result = Delete<User>(resource: USERS_RESOURCE + Path.DirectorySeparatorChar + id);
            return result.Result;
        }

        [Obsolete("Replaced by Archive(String id). Renamed for consistency with API language.")]
        public User Delete(String id)
        {
            return Archive(id);
        }

        public User UpdateLastSeenAt(String id, long timestamp)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (timestamp <= 0)
            {
                throw new ArgumentException("'timestamp' argument should be bigger than zero.");
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { id = id, last_request_at = timestamp });
            result = Post<User>(body);
            return result.Result;
        }

        public User UpdateLastSeenAt(User user, long timestamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (timestamp <= 0)
            {
                throw new ArgumentException("'timestamp' argument should be bigger than zero.");
            }

            String body = String.Empty;

            if (!String.IsNullOrEmpty(user.id))
                body = JsonConvert.SerializeObject(new { id = user.id, last_request_at = timestamp });
            else if (!String.IsNullOrEmpty(user.user_id))
                body = JsonConvert.SerializeObject(new { user_id = user.user_id, last_request_at = timestamp });
            else if (!String.IsNullOrEmpty(user.email))
                body = JsonConvert.SerializeObject(new { email = user.email, last_request_at = timestamp });
            else
                throw new ArgumentException("you need to provide either 'user.id', 'user.user_id', 'user.email' to update a user's last seen at.");

            ClientResponse<User> result = null;
            result = Post<User>(body);
            return result.Result;
        }

        public User UpdateLastSeenAt(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { id = id, update_last_request_at = true });
            result = Post<User>(body);
            return result.Result;
        }

        public User UpdateLastSeenAt(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            String body = String.Empty;

            if (!String.IsNullOrEmpty(user.id))
                body = JsonConvert.SerializeObject(new { id = user.id, update_last_request_at = true });
            else if (!String.IsNullOrEmpty(user.user_id))
                body = JsonConvert.SerializeObject(new { user_id = user.user_id, update_last_request_at = true });
            else if (!String.IsNullOrEmpty(user.email))
                body = JsonConvert.SerializeObject(new { email = user.email, update_last_request_at = true });
            else
                throw new ArgumentException("you need to provide either 'user.id', 'user.user_id', 'user.email' to update a user's last seen at.");

            ClientResponse<User> result = null;
            result = Post<User>(body);
            return result.Result;
        }

        public User IncrementUserSession(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { id = id, new_session = true });
            result = Post<User>(body);
            return result.Result;
        }

        public User IncrementUserSession(String id, List<String> companyIds)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (companyIds == null)
            {
                throw new ArgumentNullException(nameof(companyIds));
            }

            if (!companyIds.Any())
            {
                throw new ArgumentException("'companyIds' shouldnt be empty.");
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { id = id, new_session = true, companies = companyIds.Select(c => new { id = c }) });
            result = Post<User>(body);
            return result.Result;
        }

        public User RemoveCompanyFromUser(String id, List<String> companyIds)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (companyIds == null)
            {
                throw new ArgumentNullException(nameof(companyIds));
            }

            if (!companyIds.Any())
            {
                throw new ArgumentException("'companyIds' shouldnt be empty.");
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { id = id, companies = companyIds.Select(c => new { id = c, remove = true }) });
            result = Post<User>(body);
            return result.Result;
        }

        public User RemoveCompanyFromUser(User user, List<String> companyIds)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(user.id))
            {
                throw new ArgumentException("'user.id' is null.");
            }

            if (companyIds == null)
            {
                throw new ArgumentNullException(nameof(companyIds));
            }

            if (!companyIds.Any())
            {
                throw new ArgumentException("'companyIds' shouldnt be empty.");
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { id = user.id, companies = companyIds.Select(c => new { id = c, remove = true }) });
            result = Post<User>(body);
            return result.Result;
        }

        public User PermanentlyDeleteUser(String id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            ClientResponse<User> result = null;
            String body = JsonConvert.SerializeObject(new { intercom_user_id = id });
            result = Post<User>(resource: PERMANENT_DELETE_RESOURCE, body: body);
            return result.Result;
        }

        private String Transform(User user)
        {
            var companies = new object();

            if (user.companies != null && user.companies.Any())
            {
                companies = user.companies.Select(c => new
                {
                    remote_created_at = c.remote_created_at,
                    company_id = c.company_id,
                    name = c.name,
                    monthly_spend = c.monthly_spend,
                    custom_attributes = c.custom_attributes,
                    plan = c.plan != null ? c.plan.name : null,
                    website = c.website,
                    size = c.size,
                    industry = c.industry,
                    remove = c.remove
                }).ToList();
            }
            else
                companies = null;

            var body = new
            {
                id = user.id,
                user_id = user.user_id,
                email = user.email,
                phone = user.phone,
                name = user.name,
                companies = companies,
                avatar = user.avatar,
                signed_up_at = user.signed_up_at,
                last_seen_ip = user.last_seen_ip,
                custom_attributes = user.custom_attributes,
                new_session = user.new_session,
                last_seen_user_agent = user.user_agent_data,
                last_request_at = user.last_request_at,
                unsubscribed_from_emails = user.unsubscribed_from_emails,
                referrer = user.referrer,
                utm_campaign = user.utm_campaign,
                utm_content = user.utm_content,
                utm_medium = user.utm_medium,
                utm_source = user.utm_source,
                utm_term = user.utm_term
            };

            return JsonConvert.SerializeObject(body,
                           Formatting.None,
                           new JsonSerializerSettings
                           {
                               NullValueHandling = NullValueHandling.Ignore
                           });
        }
    }
}
