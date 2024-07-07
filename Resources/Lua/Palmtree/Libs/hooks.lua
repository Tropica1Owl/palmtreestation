hooks = {}
hooks.flags = {}
hooks.list = {}
hooks.blacklist = {"blacklist"}

-- -------------------------------------------------------------------------- --
--                                    Flags                                   --
-- -------------------------------------------------------------------------- --

function hooks.Flag(hook, flag, val)
	hooks.flags[hook] = hooks.flags[hook] or {}
	hooks.flags[hook][flag] = tobool(val)
end

function hooks.GetFlag(hook, flag)
	hooks.flags[hook] = hooks.flags[hook] or {}
	return hooks.flags[hook][flag] or false
end

function hooks.GetFlags(hook)
    return hooks.flags[hook] or {}
end

-- -------------------------------------------------------------------------- --
--                                   Hooking                                  --
-- -------------------------------------------------------------------------- --

hooks.Add = function(hook, callback)
	--local flags = flags or {}
	for k,v in ipairs(hooks.blacklist) do
		if (hook == v) then return end
	end

	if (hooks.list[hook] == nil) then
		hooks.list[hook] = {}
	end

	table.insert(hooks.list[hook], callback)
end



-- Gets a list of registered hooks.
hooks.GetHooks = function()
	local t = {}
	for key, _ in pairs(hooks.list) do

		if (type(hooks.list[key]) == "table") then
			local invalid = false
			for k,v in ipairs(hooks.blacklist) do
				if (key == v) then invalid = true end
			end
			if not invalid then
				table.insert(t, key)
			end

		end
	end
	return t
end

-- Fires the hook and returns all of the results from all the callbacks.
hooks.Fire = function(hook, ...)
	if (hooks.list[hook] == nil) then hooks.list[hook] = {} end
	for k,v in ipairs(hooks.blacklist) do
		if (hook == v) then return end
	end

	if (hooks.list[hook] ~= nil) then
		local t = {}
		for k,v in ipairs(hooks.list[hook]) do
			local r = v(unpack({...}))
			if (r ~= nil) then table.insert(t, r) end
		end
		return t
	end
	return nil
end

hooks.FireCheckReturn = function(hook, value, ...)
	if (hooks.list[hook] == nil) then hooks.list[hook] = {} end
	for k,v in ipairs(hooks.blacklist) do
		if (hook == v) then return end
	end

	if (hooks.list[hook] ~= nil) then
		for k,v in ipairs(hooks.list[hook]) do
			local r = v(unpack({...}))
			if (r ~= nil) then
				if (r == value) then
					return true
				end
			end
		end
	end
	return false
end

hooks.Clear = function(hook)
	for k,v in ipairs(hooks.blacklist) do
		if (hook == v) then return end
	end

	if (hooks.list[hook] ~= nil) then
		hooks.list[hook] = nil
	end
end

hooks.Remove = function(hook, callback)
	if (callback == nil) then
		if (type(hook) == "function") then
			-- Treat as callback
			local callback = hook
			local hookList = hooks.GetHooks()
			for _,h in ipairs(hookList) do
				local index = table.indexOf(hooks.list[h], callback)
				if (index ~= nil) then table.remove(hooks.list[h], index) end

				if (#hooks.list[h] == 0) then
					hooks.Clear(h)
				end
			end
		else
			hooks.Clear(hook)
		end
	else
		if (type(callback) ~= "function") then
			return
		end
		if (hooks.list[hook] ~= nil) then
			local index = table.indexOf(hooks.list[hook], callback)
			if (index ~= nil) then table.remove(hooks.list[hook], index) end

			if (#hooks.list[hook] == 0) then
				hooks.Clear(hook)
			end
		end
	end
end
