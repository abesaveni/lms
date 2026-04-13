import { useState, useEffect } from 'react'
import { Plus, Edit, Trash2, Eye, EyeOff, Search, X } from 'lucide-react'
import Button from '../../components/ui/Button'
import Input from '../../components/ui/Input'
import { Card, CardHeader, CardTitle, CardContent } from '../../components/ui/Card'
import { Badge } from '../../components/ui/Badge'
import {
  getAdminBlogs,
  createBlog,
  updateBlog,
  deleteBlog,
  updateBlogStatus,
  Blog,
  CreateBlogRequest,
} from '../../services/adminApi'
import { apiGet } from '../../services/api'

interface Category {
  id: string
  name: string
}

const BlogManagement = () => {
  const [blogs, setBlogs] = useState<Blog[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingBlog, setEditingBlog] = useState<Blog | null>(null)
  const [formData, setFormData] = useState<CreateBlogRequest>({
    title: '',
    content: '',
    summary: '',
    thumbnailUrl: '',
    categoryId: '',
    tags: '',
    isPublished: false,
  })

  useEffect(() => {
    fetchBlogs()
    fetchCategories()
  }, [])

  const fetchBlogs = async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await getAdminBlogs()
      setBlogs(data)
    } catch (err: any) {
      setError(err.message || 'Failed to fetch blogs')
    } finally {
      setIsLoading(false)
    }
  }

  const fetchCategories = async () => {
    try {
      const response = await apiGet<{ success: boolean; data?: Category[] }>('/shared/categories')
      if (response.success && response.data) {
        setCategories(response.data)
      }
    } catch (err) {
      console.error('Failed to fetch categories:', err)
    }
  }

  const handleCreate = () => {
    setEditingBlog(null)
    setFormData({
      title: '',
      content: '',
      summary: '',
      thumbnailUrl: '',
      categoryId: categories[0]?.id || '',
      tags: '',
      isPublished: false,
    })
    setShowModal(true)
  }

  const handleEdit = (blog: Blog) => {
    setEditingBlog(blog)
    setFormData({
      title: blog.title,
      content: blog.content,
      summary: blog.summary || '',
      thumbnailUrl: blog.thumbnailUrl || '',
      categoryId: blog.categoryId,
      tags: blog.tags || '',
      isPublished: blog.isPublished,
    })
    setShowModal(true)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)

    try {
      if (editingBlog) {
        await updateBlog(editingBlog.id, formData)
      } else {
        await createBlog(formData)
      }
      setShowModal(false)
      fetchBlogs()
    } catch (err: any) {
      setError(err.message || 'Failed to save blog')
    } finally {
      setIsLoading(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this blog?')) {
      return
    }

    setIsLoading(true)
    setError(null)
    try {
      await deleteBlog(id)
      fetchBlogs()
    } catch (err: any) {
      setError(err.message || 'Failed to delete blog')
    } finally {
      setIsLoading(false)
    }
  }

  const handleToggleStatus = async (id: string, currentStatus: boolean) => {
    setIsLoading(true)
    setError(null)
    try {
      await updateBlogStatus(id, !currentStatus)
      fetchBlogs()
    } catch (err: any) {
      setError(err.message || 'Failed to update blog status')
    } finally {
      setIsLoading(false)
    }
  }

  const filteredBlogs = blogs.filter((blog) =>
    blog.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    blog.content.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Blog Management</h1>
            <p className="text-gray-600">Create and manage blog posts</p>
          </div>
          <Button onClick={handleCreate}>
            <Plus className="mr-2 w-5 h-5" />
            Create Blog
          </Button>
        </div>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-sm text-red-600">
          {error}
        </div>
      )}

      {/* Search */}
      <Card className="mb-6">
        <CardContent className="pt-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <Input
              placeholder="Search blogs..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </CardContent>
      </Card>

      {/* Blogs List */}
      <Card>
        <CardHeader>
          <CardTitle>All Blogs ({filteredBlogs.length})</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading && !blogs.length ? (
            <div className="text-center py-8 text-gray-500">Loading blogs...</div>
          ) : filteredBlogs.length === 0 ? (
            <div className="text-center py-8 text-gray-500">No blogs found</div>
          ) : (
            <div className="space-y-4">
              {filteredBlogs.map((blog) => (
                <div
                  key={blog.id}
                  className="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 transition-colors"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <h3 className="text-lg font-semibold text-gray-900">{blog.title}</h3>
                        <Badge variant={blog.isPublished ? 'success' : 'default'}>
                          {blog.isPublished ? 'Published' : 'Draft'}
                        </Badge>
                      </div>
                      <p className="text-sm text-gray-600 mb-2 line-clamp-2">
                        {blog.summary || blog.content.substring(0, 150)}...
                      </p>
                      <div className="flex items-center gap-4 text-xs text-gray-500">
                        <span>Category: {blog.categoryName}</span>
                        <span>Author: {blog.authorName}</span>
                        <span>Views: {blog.viewCount}</span>
                        <span>
                          {blog.isPublished
                            ? `Published: ${new Date(blog.publishedAt!).toLocaleDateString()}`
                            : `Created: ${new Date(blog.createdAt).toLocaleDateString()}`}
                        </span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 ml-4">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleToggleStatus(blog.id, blog.isPublished)}
                        title={blog.isPublished ? 'Unpublish' : 'Publish'}
                      >
                        {blog.isPublished ? (
                          <EyeOff className="w-4 h-4" />
                        ) : (
                          <Eye className="w-4 h-4" />
                        )}
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleEdit(blog)}
                        title="Edit"
                      >
                        <Edit className="w-4 h-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDelete(blog.id)}
                        title="Delete"
                      >
                        <Trash2 className="w-4 h-4 text-red-600" />
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <Card className="w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>{editingBlog ? 'Edit Blog' : 'Create Blog'}</CardTitle>
                <Button variant="ghost" size="sm" onClick={() => setShowModal(false)}>
                  <X className="w-5 h-5" />
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Title *
                  </label>
                  <Input
                    value={formData.title}
                    onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Summary
                  </label>
                  <Input
                    value={formData.summary}
                    onChange={(e) => setFormData({ ...formData, summary: e.target.value })}
                    placeholder="Brief summary of the blog"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Content *
                  </label>
                  <textarea
                    value={formData.content}
                    onChange={(e) => setFormData({ ...formData, content: e.target.value })}
                    required
                    rows={10}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Category *
                    </label>
                    <select
                      value={formData.categoryId}
                      onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                      required
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    >
                      <option value="">Select category</option>
                      {categories.map((cat) => (
                        <option key={cat.id} value={cat.id}>
                          {cat.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Thumbnail URL
                    </label>
                    <Input
                      value={formData.thumbnailUrl}
                      onChange={(e) => setFormData({ ...formData, thumbnailUrl: e.target.value })}
                      placeholder="https://example.com/image.jpg"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Tags (comma-separated)
                  </label>
                  <Input
                    value={formData.tags}
                    onChange={(e) => setFormData({ ...formData, tags: e.target.value })}
                    placeholder="tag1, tag2, tag3"
                  />
                </div>

                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="isPublished"
                    checked={formData.isPublished}
                    onChange={(e) => setFormData({ ...formData, isPublished: e.target.checked })}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <label htmlFor="isPublished" className="text-sm font-medium text-gray-700">
                    Publish immediately
                  </label>
                </div>

                <div className="flex justify-end gap-2 pt-4">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setShowModal(false)}
                    disabled={isLoading}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" disabled={isLoading}>
                    {isLoading ? 'Saving...' : editingBlog ? 'Update' : 'Create'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}

export default BlogManagement
